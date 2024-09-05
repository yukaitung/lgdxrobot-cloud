using AutoMapper;
using Grpc.Core;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.Utilities.Constants;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static LGDXRobot2Cloud.Protos.RobotClientsService;

namespace LGDXRobot2Cloud.API.Services;

[Authorize(AuthenticationSchemes = LgdxRobot2AuthenticationSchemes.RobotClientsJwtScheme)]
public class RobotClientsService(IOnlineRobotsService OnlineRobotsService,
  IAutoTaskDetailRepository autoTaskDetailRepository,
  IAutoTaskSchedulerService autoTaskSchedulerService,
  IMapper mapper,
  IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
  IRobotChassisInfoRepository robotChassisInfoRepository,
  IRobotRepository robotRepository,
  IRobotSystemInfoRepository robotSystemInfoRepository) : RobotClientsServiceBase
{
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService ?? throw new ArgumentNullException(nameof(OnlineRobotsService));
  private readonly IAutoTaskDetailRepository _autoTaskDetailRepository = autoTaskDetailRepository ?? throw new ArgumentNullException(nameof(autoTaskDetailRepository));
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly IRobotChassisInfoRepository _robotChassisInfoRepository = robotChassisInfoRepository ?? throw new ArgumentNullException(nameof(robotChassisInfoRepository));
  private readonly IRobotRepository _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));
  private readonly IRobotSystemInfoRepository _robotSystemInfoRepository = robotSystemInfoRepository ?? throw new ArgumentNullException(nameof(robotSystemInfoRepository));
  private readonly LgdxRobot2SecretConfiguration _lgdxRobot2SecretConfiguration = lgdxRobot2SecretConfiguration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2SecretConfiguration));

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
      Message = "Robot ID is missing in the certificate."
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
      Message = "The robot is offline."
    };
  }

  private async Task<RobotClientsAutoTask?> GenerateTaskDetail(AutoTask? task)
  {
    if (task == null)
      return null;

    List<RobotClientsDof> waypoints = [];
    if (task.CurrentProgressId == (int)ProgressState.PreMoving)
    {
      var firstTaskDetail = await _autoTaskDetailRepository.GetAutoTaskFirstDetailAsync(task.Id);
      if (firstTaskDetail != null)
        waypoints.Add(GenerateWaypoint(firstTaskDetail));
    }
    else if (task.CurrentProgressId == (int)ProgressState.Moving)
    {
      var taskDetails = await _autoTaskDetailRepository.GetAutoTaskDetailsAsync(task.Id);
      foreach (var t in taskDetails)
      {
        if (t.Waypoint != null)
          waypoints.Add(GenerateWaypoint(t));
      }
    }

    return new RobotClientsAutoTask {
      TaskId = task.Id,
      TaskName = task.Name ?? string.Empty,
      TaskProgressId = task.CurrentProgressId,
      TaskProgressName = task.CurrentProgress.Name ?? string.Empty,
      Waypoints = { waypoints },
      NextToken = task.NextToken
    };
  }

  private static RobotClientsDof GenerateWaypoint(AutoTaskDetail taskDetail)
  {
    if (taskDetail.Waypoint != null)
    {
      var waypoint = new RobotClientsDof 
        { X = taskDetail.Waypoint.X, Y = taskDetail.Waypoint.Y, Rotation = taskDetail.Waypoint.Rotation };
      if (taskDetail.CustomX != null)
        waypoint.X = (double)taskDetail.CustomX;
      if (taskDetail.CustomY != null)
        waypoint.X = (double)taskDetail.CustomY;
      if (taskDetail.CustomRotation != null)
        waypoint.X = (double)taskDetail.CustomRotation;
      return waypoint;
    }
    else 
    {
      return new RobotClientsDof { 
        X = taskDetail.CustomX != null ? (double)taskDetail.CustomX : 0, 
        Y = taskDetail.CustomY != null ? (double)taskDetail.CustomY : 0, 
        Rotation = taskDetail.CustomRotation != null ? (double)taskDetail.CustomRotation : 0 };
    }
  }

  [Authorize(AuthenticationSchemes = LgdxRobot2AuthenticationSchemes.RobotClientsCertificateScheme)]
  public override async Task<RobotClientsGreetRespond> Greet(RobotClientsGreet request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return new RobotClientsGreetRespond {
        Status = RobotClientsResultStatus.Failed,
        Message = "Robot ID is missing in the certificate.",
        AccessToken = string.Empty
      };

    var robot = await _robotRepository.GetRobotAsync((Guid)robotId);
    if (robot == null)
      return new RobotClientsGreetRespond {
        Status = RobotClientsResultStatus.Failed,
        Message = "Robot not found.",
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
      Is32Bit =  request.SystemInfo.Is32Bit,
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
          Message = "Motherboard serial number is mismatched.",
          AccessToken = string.Empty
        };
      }
      _mapper.Map(incomingSystemInfoEntity, systemInfoEntity);
    }
    await _robotSystemInfoRepository.SaveChangesAsync();

    var incomingChassisInfoEntity = new RobotChassisInfo {
      McuSerialNumber = request.ChassisInfo.McuSerialNumber,
      ChassisLX = request.ChassisInfo.ChassisLX,
      ChassisLY = request.ChassisInfo.ChassisLY,
      ChassisWheelCount = request.ChassisInfo.ChassisWheelCount,
      ChassisWheelRadius = request.ChassisInfo.ChassisWheelRadius,
      BatteryCount = request.ChassisInfo.BatteryCount,
      BatteryMaxVoltage = request.ChassisInfo.BatteryMaxVoltage,
      BatteryMinVoltage = request.ChassisInfo.BatteryMinVoltage,
      RobotId = (Guid)robotId
    };
    var chassisInfoEntity = await _robotChassisInfoRepository.GetChassisInfoAsync((Guid)robotId);
    if (chassisInfoEntity == null)
      await _robotChassisInfoRepository.AddChassisInfoAsync(incomingChassisInfoEntity);
    else 
    {
      // Hardware Protection
      if (robot.IsProtectingHardwareSerialNumber && incomingChassisInfoEntity.McuSerialNumber != chassisInfoEntity.McuSerialNumber)
      {
        return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          Message = "MCU serial number is mismatched.",
          AccessToken = string.Empty
        };
      }
      _mapper.Map(incomingChassisInfoEntity, chassisInfoEntity);
    }
    await _robotChassisInfoRepository.SaveChangesAsync();

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobot2SecretConfiguration.RobotClientsJwtSecret));
    var credentials = new SigningCredentials(securityKey, _lgdxRobot2SecretConfiguration.RobotClientsJwtAlgorithm);
    var secToken = new JwtSecurityToken(
      _lgdxRobot2SecretConfiguration.RobotClientsJwtIssuer,
      _lgdxRobot2SecretConfiguration.RobotClientsJwtIssuer,
      [new Claim(ClaimTypes.NameIdentifier, robot.Id.ToString())],
      DateTime.UtcNow,
      DateTime.UtcNow.AddMinutes(_lgdxRobot2SecretConfiguration.RobotClientsJwtExpireMins),
      credentials);
    var token =  new JwtSecurityTokenHandler().WriteToken(secToken);

    _onlineRobotsService.AddRobot((Guid)robotId);

    return new RobotClientsGreetRespond {
      Status = RobotClientsResultStatus.Success,
      Message = string.Empty,
      AccessToken = token
    };
  }

  public override async Task<RobotClientsRespond> Exchange(RobotClientsExchange request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!ValidateOnlineRobots((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    _onlineRobotsService.SetRobotData((Guid)robotId, request);

    // Get AutoTask
    if (request.RobotStatus == RobotClientsRobotStatus.Idle)
    {
      var task = await _autoTaskSchedulerService.GetAutoTask((Guid)robotId);
      var taskDetail = await GenerateTaskDetail(task);
      return new RobotClientsRespond {
        Status = RobotClientsResultStatus.Success,
        Message = string.Empty,
        Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
        Task = taskDetail
      };
    }
    else
    {
      return new RobotClientsRespond {
        Status = RobotClientsResultStatus.Success,
        Message = string.Empty,
        Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      };
    }
  }

  public override async Task<RobotClientsRespond> AutoTaskNext(RobotClientsNextToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!ValidateOnlineRobots((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    var (task, errorMessage) = await _autoTaskSchedulerService.AutoTaskNext((Guid)robotId, request.TaskId, request.NextToken);
    var taskDetail = await GenerateTaskDetail(task);
    return new RobotClientsRespond {
      Status = errorMessage == string.Empty ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Message = errorMessage,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      Task = taskDetail
    };
  }

  public override async Task<RobotClientsRespond> AutoTaskAbort(RobotClientsNextToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!ValidateOnlineRobots((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    var result = await _autoTaskSchedulerService.AutoTaskAbort((Guid)robotId, request.TaskId, request.NextToken);
    return new RobotClientsRespond {
      Status = result == string.Empty ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Message = result,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
    };
  }
}