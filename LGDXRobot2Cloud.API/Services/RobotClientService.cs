using System.Security.Claims;
using AutoMapper;
using Grpc.Core;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Enums;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using static LGDXRobot2Cloud.Protos.RobotClientService;

namespace LGDXRobot2Cloud.API.Services;

[Authorize(AuthenticationSchemes = CertificateAuthenticationDefaults.AuthenticationScheme)]
public class RobotClientService(IAutoTaskSchedulerService autoTaskSchedulerService,
  IAutoTaskDetailRepository autoTaskDetailRepository,
  IRobotSystemInfoRepository robotSystemInfoRepository,
  IMemoryCache memoryCache,
  IMapper mapper) : RobotClientServiceBase
{
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
  private readonly IAutoTaskDetailRepository _autoTaskDetailRepository = autoTaskDetailRepository ?? throw new ArgumentNullException(nameof(autoTaskDetailRepository));
  private readonly IRobotSystemInfoRepository _robotSystemInfoRepository = robotSystemInfoRepository ?? throw new ArgumentNullException(nameof(robotSystemInfoRepository));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

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

  static private RobotData GenerateRobotData(RobotClientExchange data)
  {
    var batteriesCount = data.Batteries.Count;
    (double, double, double, double) batteries = (0, 0, 0, 0);
    if (batteriesCount >= 1)
      batteries.Item1 = data.Batteries.ElementAt(0);
    if (batteriesCount >= 2)
      batteries.Item2 = data.Batteries.ElementAt(1);
    if (batteriesCount >= 3)
      batteries.Item3 = data.Batteries.ElementAt(2);
    if (batteriesCount >= 4)
      batteries.Item4 = data.Batteries.ElementAt(3);
    /*
    var emergencyStopsCount = data.EmergencyStopsEnabled.Count;
    (bool, bool) emergencyStopsEnabled = (false, false);
    if (emergencyStopsCount >= 1)
      emergencyStopsEnabled.Item1 = data.EmergencyStopsEnabled.ElementAt(0);
    if (emergencyStopsCount >= 2)
      emergencyStopsEnabled.Item2 = data.EmergencyStopsEnabled.ElementAt(1);
    */

    return new RobotData {
      Batteries = batteries,
      Position = (data.Position.X, data.Position.Y, data.Position.Rotation),
      //EmergencyStopsEnabled = emergencyStopsEnabled,
      Eta = data.NavProgress.Eta,
      Recoveries = data.NavProgress.Recoveries,
      DistanceRemaining = data.NavProgress.DistanceRemaining,
      WaypointsRemaining = data.NavProgress.WaypointsRemaining
    };
  }

  public override async Task<RobotClientGreetRespond> Greet(RobotClientGreet request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    //if (robotId == null)
    //  return ValidateRobotClaimFailed();

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
      _mapper.Map(incomingSystemInfoEntity, systemInfoEntity);
    await _robotSystemInfoRepository.SaveChangesAsync();

    // TODO: Generate RobotClientChassisInfo

    // TODO: Generate BearerToken

    return new RobotClientGreetRespond {
      BearerToken = string.Empty
    };
  }

  public override async Task<RobotClientRespond> Exchange(RobotClientExchange request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();

    _memoryCache.Set($"gRPCRobotData_{robotId}", GenerateRobotData(request), TimeSpan.FromMinutes(5));

    // Get AutoTask
    if (request.GetTask)
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