using AutoMapper;
using Grpc.Core;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.Utilities.Constants;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static LGDXRobot2Cloud.Protos.RobotClientsService;

namespace LGDXRobot2Cloud.API.Services;

[Authorize(AuthenticationSchemes = LgdxRobot2AuthenticationSchemes.RobotClientsJwtScheme)]
public class RobotClientsService(
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IMapper mapper,
    IOnlineRobotsService OnlineRobotsService,
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    IRobotChassisInfoRepository robotChassisInfoRepository,
    IRobotRepository robotRepository,
    IRobotSystemInfoRepository robotSystemInfoRepository
  ) : RobotClientsServiceBase
{
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService;
  private readonly IMapper _mapper = mapper;
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService;
  private readonly IRobotChassisInfoRepository _robotChassisInfoRepository = robotChassisInfoRepository;
  private readonly IRobotRepository _robotRepository = robotRepository;
  private readonly IRobotSystemInfoRepository _robotSystemInfoRepository = robotSystemInfoRepository;
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value;

  private static Guid? ValidateRobotClaim(ServerCallContext context)
  {
    var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
    if (robotClaim == null)
    {
      return null;
    }
    return Guid.Parse(robotClaim.Value);
  }

  private static RobotClientsRespond ValidateRobotClaimFailed()
  {
    return new RobotClientsRespond {
      Status = RobotClientsResultStatus.Failed,
    };
  }

  // TODO: Validate in authorisation
  private bool ValidateOnlineRobots(Guid robotId)
  {
    return _onlineRobotsService.IsRobotOnline(robotId);
  }

  private static RobotClientsRespond ValidateOnlineRobotsFailed()
  {
    return new RobotClientsRespond {
      Status = RobotClientsResultStatus.Failed,
    };
  }

  [Authorize(AuthenticationSchemes = LgdxRobot2AuthenticationSchemes.RobotClientsCertificateScheme)]
  public override async Task<RobotClientsGreetRespond> Greet(RobotClientsGreet request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return new RobotClientsGreetRespond {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    var robot = await _robotRepository.GetRobotAsync((Guid)robotId);
    if (robot == null)
      return new RobotClientsGreetRespond {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    var incomingSystemInfoEntity = new RobotSystemInfo {
      Cpu = request.SystemInfo.Cpu,
      IsLittleEndian =  request.SystemInfo.IsLittleEndian,
      Motherboard = request.SystemInfo.Motherboard,
      MotherboardSerialNumber = request.SystemInfo.MotherboardSerialNumber,
      RamMiB =  request.SystemInfo.RamMiB,
      Gpu =  request.SystemInfo.Gpu,
      Os =  request.SystemInfo.Os,
      Is32Bit = request.SystemInfo.Is32Bit,
      McuSerialNumber = request.SystemInfo.McuSerialNumber,
      RobotId = (Guid)robotId
    };
    var systemInfoEntity = await _robotSystemInfoRepository.GetRobotSystemInfoAsync((Guid)robotId);
    if (systemInfoEntity == null)
      await _robotSystemInfoRepository.AddRobotSystemInfoAsync(incomingSystemInfoEntity);
    else 
    {
      // Hardware Protection
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfoEntity.MotherboardSerialNumber != systemInfoEntity.MotherboardSerialNumber)
      {
        return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfoEntity.McuSerialNumber != systemInfoEntity.McuSerialNumber)
      {
        return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      _mapper.Map(incomingSystemInfoEntity, systemInfoEntity);
    }
    await _robotSystemInfoRepository.SaveChangesAsync();

    var robotChassisInfo = await _robotChassisInfoRepository.GetChassisInfoAsync((Guid)robotId);
    if (robotChassisInfo == null) {
      return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
    }

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobot2SecretConfiguration.RobotClientsJwtSecret));
    var credentials = new SigningCredentials(securityKey, _lgdxRobot2SecretConfiguration.RobotClientsJwtAlgorithm);
    var secToken = new JwtSecurityToken(
      _lgdxRobot2SecretConfiguration.RobotClientsJwtIssuer,
      _lgdxRobot2SecretConfiguration.RobotClientsJwtIssuer,
      [new Claim(ClaimTypes.NameIdentifier, robot.Id.ToString())],
      DateTime.UtcNow,
      DateTime.UtcNow.AddMinutes(_lgdxRobot2SecretConfiguration.RobotClientsJwtExpireMins),
      credentials);
    var token = new JwtSecurityTokenHandler().WriteToken(secToken);

    _onlineRobotsService.AddRobot((Guid)robotId);

    return new RobotClientsGreetRespond {
      Status = RobotClientsResultStatus.Success,
      AccessToken = token,
      ChassisInfo = new RobotClientsChassisInfo {
        ChassisLX = robotChassisInfo.ChassisLengthX,
        ChassisLY = robotChassisInfo.ChassisLengthY,
        ChassisWheelCount = robotChassisInfo.ChassisWheelCount,
        ChassisWheelRadius = robotChassisInfo.ChassisWheelRadius,
        BatteryCount = robotChassisInfo.BatteryCount,
        BatteryMaxVoltage = robotChassisInfo.BatteryMaxVoltage,
        BatteryMinVoltage = robotChassisInfo.BatteryMinVoltage
      }
    };
  }

  public override async Task<RobotClientsRespond> Exchange(RobotClientsExchange request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!ValidateOnlineRobots((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    await _onlineRobotsService.UpdateRobotDataAsync((Guid)robotId, request);
    var manualAutoTask = _onlineRobotsService.GetAutoTaskNext((Guid)robotId);
    if (manualAutoTask != null)
    {
      // Triggered by API
      return new RobotClientsRespond {
        Status = RobotClientsResultStatus.Success,
        Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
        Task = await _autoTaskSchedulerService.AutoTaskNextManualAsync(manualAutoTask)
      };
    }
    else
    {
      // Get AutoTask
      if (request.RobotStatus == RobotClientsRobotStatus.Idle)
      {
        var task = await _autoTaskSchedulerService.GetAutoTaskAsync((Guid)robotId);
        return new RobotClientsRespond {
          Status = RobotClientsResultStatus.Success,
          Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
          Task = task
        };
      }
      else
      {
        return new RobotClientsRespond {
          Status = RobotClientsResultStatus.Success,
          Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
        };
      }
    }
  }

  public override async Task<RobotClientsRespond> AutoTaskNext(RobotClientsNextToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!ValidateOnlineRobots((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    var task = await _autoTaskSchedulerService.AutoTaskNextAsync((Guid)robotId, request.TaskId, request.NextToken);
    return new RobotClientsRespond {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      Task = task
    };
  }

  public override async Task<RobotClientsRespond> AutoTaskAbort(RobotClientsNextToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!ValidateOnlineRobots((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    await _onlineRobotsService.SetAbortTaskAsync((Guid)robotId, false);

    var task = await _autoTaskSchedulerService.AutoTaskAbortAsync((Guid)robotId, request.TaskId, request.NextToken);
    return new RobotClientsRespond {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      Task = task
    };
  }
}