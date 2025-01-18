using Grpc.Core;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Services.Navigation;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Utilities.Constants;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static LGDXRobot2Cloud.Protos.RobotClientsService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LGDXRobot2Cloud.Data.Models.Business.Navigation;

namespace LGDXRobot2Cloud.API.Services;

[Authorize(AuthenticationSchemes = LgdxRobot2AuthenticationSchemes.RobotClientsJwtScheme)]
public class RobotClientsService(
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IOnlineRobotsService OnlineRobotsService,
    IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
    IRobotService robotService
  ) : RobotClientsServiceBase
{
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService;
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService;
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value;
  private readonly IRobotService _robotService = robotService;

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
  private async Task<bool> ValidateOnlineRobotsAsync(Guid robotId)
  {
    return await _onlineRobotsService.IsRobotOnlineAsync(robotId);
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

    var robotIdGuid = (Guid)robotId;
    var robot = await _robotService.GetRobotAsync(robotIdGuid);
    if (robot == null)
      return new RobotClientsGreetRespond {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    // Compare System Info
    var incomingSystemInfo = new RobotSystemInfoBusinessModel {
      Id = 0,
      Cpu = request.SystemInfo.Cpu,
      IsLittleEndian =  request.SystemInfo.IsLittleEndian,
      Motherboard = request.SystemInfo.Motherboard,
      MotherboardSerialNumber = request.SystemInfo.MotherboardSerialNumber,
      RamMiB =  request.SystemInfo.RamMiB,
      Gpu =  request.SystemInfo.Gpu,
      Os =  request.SystemInfo.Os,
      Is32Bit = request.SystemInfo.Is32Bit,
      McuSerialNumber = request.SystemInfo.McuSerialNumber,
    };
    var systemInfo = await _robotService.GetRobotSystemInfoAsync(robotIdGuid);
    if (systemInfo == null)
    {
      // Create Robot System Info for the first time
      await _robotService.CreateRobotSystemInfoAsync(robotIdGuid, incomingSystemInfo.ToCreateBusinessModel());
    }
    else 
    {
      // Hardware Protection
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfo.MotherboardSerialNumber != systemInfo.MotherboardSerialNumber)
      {
        return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfo.McuSerialNumber != systemInfo.McuSerialNumber)
      {
        return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      await _robotService.UpdateRobotSystemInfoAsync(robotIdGuid, incomingSystemInfo.ToUpdateBusinessModel());
    }

    // Obtain Robot Chassis Info
    var robotChassisInfo = await _robotService.GetRobotChassisInfoAsync((Guid)robotId);
    if (robotChassisInfo == null) {
      return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
    }

    // Generate Access Token
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

    await _onlineRobotsService.AddRobotAsync((Guid)robotId);

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
    if (!await ValidateOnlineRobotsAsync((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    await _onlineRobotsService.UpdateRobotDataAsync((Guid)robotId, request);
    var manualAutoTask = _onlineRobotsService.GetAutoTaskNextApi((Guid)robotId);
    if (manualAutoTask != null)
    {
      // Triggered by API
      return new RobotClientsRespond {
        Status = RobotClientsResultStatus.Success,
        Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
        Task = await _autoTaskSchedulerService.AutoTaskNextConstructAsync(manualAutoTask)
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
    if (!await ValidateOnlineRobotsAsync((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    var task = await _autoTaskSchedulerService.AutoTaskNextAsync((Guid)robotId, request.TaskId, request.NextToken);
    return new RobotClientsRespond {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      Task = task
    };
  }

  public override async Task<RobotClientsRespond> AutoTaskAbort(RobotClientsAbortToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!await ValidateOnlineRobotsAsync((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    await _onlineRobotsService.SetAbortTaskAsync((Guid)robotId, false);

    AutoTaskAbortReason autoTaskAbortReason = (AutoTaskAbortReason)(int)request.AbortReason;
    var task = await _autoTaskSchedulerService.AutoTaskAbortAsync((Guid)robotId, request.TaskId, request.NextToken, autoTaskAbortReason);
    return new RobotClientsRespond {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      Task = task
    };
  }
}