using AutoMapper;
using Grpc.Core;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Constants;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static LGDXRobot2Cloud.Protos.RobotClientService;

namespace LGDXRobot2Cloud.API.Services;

[Authorize(AuthenticationSchemes = LgdxRobot2AuthenticationSchemes.RobotClientJwtScheme)]
public class RobotClientService(IAutoTaskDetailRepository autoTaskDetailRepository,
  IAutoTaskSchedulerService autoTaskSchedulerService,
  IMapper mapper,
  IMemoryCache memoryCache,
  IOptionsSnapshot<LgdxRobot2SecretConfiguration> lgdxRobot2SecretConfiguration,
  IRobotChassisInfoRepository robotChassisInfoRepository,
  IRobotRepository robotRepository,
  IRobotSystemInfoRepository robotSystemInfoRepository) : RobotClientServiceBase
{
  private readonly IAutoTaskDetailRepository _autoTaskDetailRepository = autoTaskDetailRepository ?? throw new ArgumentNullException(nameof(autoTaskDetailRepository));
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
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

  private static RobotClientRespond ValidateRobotClaimFailed()
  {
    return new RobotClientRespond {
      Status = RobotClientResultStatus.Failed,
      Message = "Robot ID is missing in the certificate."
    };
  }

  private async Task<RobotClientAutoTask?> GenerateTaskDetail(AutoTask? task)
  {
    if (task == null)
      return null;

    List<RobotClientDof> waypoints = [];
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

    return new RobotClientAutoTask {
      TaskId = task.Id,
      TaskName = task.Name ?? string.Empty,
      TaskProgressId = task.CurrentProgressId,
      TaskProgressName = task.CurrentProgress.Name ?? string.Empty,
      Waypoints = { waypoints },
      NextToken = task.NextToken
    };
  }

  private static RobotClientDof GenerateWaypoint(AutoTaskDetail taskDetail)
  {
    if (taskDetail.Waypoint != null)
    {
      var waypoint = new RobotClientDof 
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
      return new RobotClientDof { 
        X = taskDetail.CustomX != null ? (double)taskDetail.CustomX : 0, 
        Y = taskDetail.CustomY != null ? (double)taskDetail.CustomY : 0, 
        Rotation = taskDetail.CustomRotation != null ? (double)taskDetail.CustomRotation : 0 };
    }
  }

  [Authorize(AuthenticationSchemes = LgdxRobot2AuthenticationSchemes.RobotClientCertificateScheme)]
  public override async Task<RobotClientGreetRespond> Greet(RobotClientGreet request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return new RobotClientGreetRespond {
        Status = RobotClientResultStatus.Failed,
        Message = "Robot ID is missing in the certificate.",
        AccessToken = string.Empty
      };

    var robot = await _robotRepository.GetRobotAsync((Guid)robotId);
    if (robot == null)
      return new RobotClientGreetRespond {
        Status = RobotClientResultStatus.Failed,
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
        return new RobotClientGreetRespond {
          Status = RobotClientResultStatus.Failed,
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
        return new RobotClientGreetRespond {
          Status = RobotClientResultStatus.Failed,
          Message = "MCU serial number is mismatched.",
          AccessToken = string.Empty
        };
      }
      _mapper.Map(incomingChassisInfoEntity, chassisInfoEntity);
    }
    await _robotChassisInfoRepository.SaveChangesAsync();

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobot2SecretConfiguration.RobotClientJwtSecret));
    var credentials = new SigningCredentials(securityKey, _lgdxRobot2SecretConfiguration.RobotClientJwtAlgorithm);
    var secToken = new JwtSecurityToken(
      _lgdxRobot2SecretConfiguration.RobotClientJwtIssuer,
      _lgdxRobot2SecretConfiguration.RobotClientJwtIssuer,
      [new Claim(ClaimTypes.NameIdentifier, robot.Id.ToString())],
      DateTime.UtcNow,
      DateTime.UtcNow.AddMinutes(_lgdxRobot2SecretConfiguration.RobotClientJwtExpireMins),
      credentials);
    var token =  new JwtSecurityTokenHandler().WriteToken(secToken);

    _memoryCache.TryGetValue<HashSet<Guid>>("RobotClientService_ActiveRobots", out var activeRobotIds);
    activeRobotIds ??= [];
    activeRobotIds.Add((Guid)robotId);
    _memoryCache.Set("RobotClientService_ActiveRobots", activeRobotIds, TimeSpan.FromDays(1));

    return new RobotClientGreetRespond {
      Status = RobotClientResultStatus.Success,
      Message = string.Empty,
      AccessToken = token
    };
  }

  public override async Task<RobotClientRespond> Exchange(RobotClientExchange request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();

    _memoryCache.Set($"RobotClientService_RobotData_{robotId}", request, TimeSpan.FromMinutes(5));

    // Get AutoTask
    if (request.RobotStatus == RobotClientRobotStatus.Idle)
    {
      var task = await _autoTaskSchedulerService.GetAutoTask((Guid)robotId);
      var taskDetail = await GenerateTaskDetail(task);
      return new RobotClientRespond {
        Status = RobotClientResultStatus.Success,
        Message = string.Empty,
        Task = taskDetail
      };
    }
    else
    {
      return new RobotClientRespond {
        Status = RobotClientResultStatus.Success,
        Message = string.Empty,
      };
    }
  }

  public override async Task<RobotClientRespond> AutoTaskNext(RobotClientNextToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();

    var (task, errorMessage) = await _autoTaskSchedulerService.AutoTaskNext((Guid)robotId, request.TaskId, request.NextToken);
    var taskDetail = await GenerateTaskDetail(task);
    return new RobotClientRespond {
      Status = errorMessage == string.Empty ? RobotClientResultStatus.Success : RobotClientResultStatus.Failed,
      Message = errorMessage,
      Task = taskDetail
    };
  }

  public override async Task<RobotClientRespond> AutoTaskAbort(RobotClientNextToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();

    var result = await _autoTaskSchedulerService.AutoTaskAbort((Guid)robotId, request.TaskId, request.NextToken);
    return new RobotClientRespond {
      Status = result == string.Empty ? RobotClientResultStatus.Success : RobotClientResultStatus.Failed,
      Message = result
    };
  }
}