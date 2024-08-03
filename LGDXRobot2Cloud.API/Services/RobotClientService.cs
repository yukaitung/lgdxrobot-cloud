using System.Security.Claims;
using Grpc.Core;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Utilities;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authorization;
using static LGDXRobot2Cloud.Protos.RobotClientService;

namespace LGDXRobot2Cloud.API.Services
{
  [Authorize(AuthenticationSchemes = CertificateAuthenticationDefaults.AuthenticationScheme)]
  public class RobotClientService(IAutoTaskSchedulerService autoTaskSchedulerService,
    IAutoTaskDetailRepository autoTaskDetailRepository,
    IRobotDataService robotDataService,
    IRobotSystemInfoRepository robotSystemInfoRepository) : RobotClientServiceBase
  {
    private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
    private readonly IAutoTaskDetailRepository _autoTaskDetailRepository = autoTaskDetailRepository ?? throw new ArgumentNullException(nameof(autoTaskDetailRepository));
    private readonly IRobotDataService _robotDataService = robotDataService ?? throw new ArgumentNullException(nameof(robotDataService));
    private readonly IRobotSystemInfoRepository _robotSystemInfoRepository = robotSystemInfoRepository ?? throw new ArgumentNullException(nameof(robotSystemInfoRepository));

    private static Guid? ValidateRobotClaim(ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return null;
      }
      return Guid.Parse(robotClaim.Value);
    }

    private static RpcRespond ValidateRobotClaimFailed()
    {
      return new RpcRespond {
        Status = RpcResultStatus.Failed,
        Message = "Robot ID missing in CN field."
      };
    }

    private async Task<RpcAutoTask?> GenerateTaskDetail(AutoTask? task)
    {
      if (task == null)
        return null;

      List<RpcRobotDof> waypoints = [];
      if (task.CurrentProgressId == (int)ProgressState.PreMoving)
      {
        var firstTaskDetail = await _autoTaskDetailRepository.GetAutoTaskFirstDetailAsync(task.Id);
        if (firstTaskDetail != null)
        {
          waypoints.Add(new RpcRobotDof {X = firstTaskDetail.Waypoint.X, Y = firstTaskDetail.Waypoint.Y, W = firstTaskDetail.Waypoint.W});
        }
      }
      if (task.CurrentProgressId == (int)ProgressState.Moving)
      {
        var taskDetails = await _autoTaskDetailRepository.GetAutoTaskDetailsAsync(task.Id);
        foreach (var t in taskDetails)
        {
          waypoints.Add(new RpcRobotDof {X = t.Waypoint.X, Y = t.Waypoint.Y, W = t.Waypoint.W});
        }
      }
      return new RpcAutoTask{
        TaskId = task.Id,
        TaskName = task.Name ?? string.Empty,
        TaskProgressId = task.CurrentProgressId,
        TaskProgressName = task.CurrentProgress.Name ?? string.Empty,
        Waypoints = {waypoints},
        NextToken = task.NextToken
      };
    }

    static private RobotData GenerateRobotData(RpcExchange data)
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

      var emergencyStopsCount = data.EmergencyStopsEnabled.Count;
      (bool, bool) emergencyStopsEnabled = (false, false);
      if (emergencyStopsCount >= 1)
        emergencyStopsEnabled.Item1 = data.EmergencyStopsEnabled.ElementAt(0);
      if (emergencyStopsCount >= 2)
        emergencyStopsEnabled.Item2 = data.EmergencyStopsEnabled.ElementAt(1);

      return new RobotData {
        Batteries = batteries,
        Position = (data.Position.X, data.Position.Y, data.Position.W),
        EmergencyStopsEnabled = emergencyStopsEnabled,
        Eta = data.NavProgress.Eta,
        Recoveries = data.NavProgress.Recoveries,
        DistanceRemaining = data.NavProgress.DistanceRemaining,
        WaypointsRemaining = data.NavProgress.WaypointsRemaining
      };
    }

    public override async Task<RpcRespond> Greet(RpcGreet greet, ServerCallContext context)
    {
      var robotId = ValidateRobotClaim(context);
      if (robotId == null)
        return ValidateRobotClaimFailed();

      bool success;
      var systemInfoEntity = await _robotSystemInfoRepository.GetRobotSystemInfoAsync((Guid)robotId);
      if (systemInfoEntity == null)
      {
        var newSystemInfoEntity = new RobotSystemInfo {
          Cpu = greet.SystemInfo.Cpu,
          IsLittleEndian =  greet.SystemInfo.IsLittleEndian,
          RamMiB =  greet.SystemInfo.RamMiB,
          Gpu =  greet.SystemInfo.Gpu,
          Os =  greet.SystemInfo.Os,
          Is32Bit =  greet.SystemInfo.Is32Bit,
          RobotId = (Guid)robotId
        };
        await _robotSystemInfoRepository.AddRobotSystemInfoAsync(newSystemInfoEntity);
      }
      else
      {
        systemInfoEntity.Cpu = greet.SystemInfo.Cpu;
        systemInfoEntity.IsLittleEndian = greet.SystemInfo.IsLittleEndian;
        systemInfoEntity.RamMiB = greet.SystemInfo.RamMiB;
        systemInfoEntity.Gpu = greet.SystemInfo.Gpu;
        systemInfoEntity.Os = greet.SystemInfo.Os;
        systemInfoEntity.Is32Bit = greet.SystemInfo.Is32Bit;
      }

      success = await _robotSystemInfoRepository.SaveChangesAsync();
      return new RpcRespond {
        Status = success ? RpcResultStatus.Success : RpcResultStatus.Failed,
        Message = success ? string.Empty : "Database error."
      };
    }

    public override async Task<RpcRespond> Exchange(RpcExchange data, ServerCallContext context)
    {
      var robotId = ValidateRobotClaim(context);
      if (robotId == null)
        return ValidateRobotClaimFailed();

      _robotDataService.SetRobotData((Guid)robotId, GenerateRobotData(data));
      // Get AutoTask
      if (data.GetTask == true)
      {
        var task = await _autoTaskSchedulerService.GetAutoTask((Guid)robotId);
        var taskDetail = await GenerateTaskDetail(task);
        return new RpcRespond {
          Status = RpcResultStatus.Success,
          Message = string.Empty,
          Task = taskDetail
        };
      }
      else
      {
        return new RpcRespond {
          Status = RpcResultStatus.Success,
          Message = string.Empty,
        };
      }
    }

    public override async Task<RpcRespond> AutoTaskNext(RpcNextToken token, ServerCallContext context)
    {
      var robotId = ValidateRobotClaim(context);
      if (robotId == null)
        return ValidateRobotClaimFailed();

      var (task, errorMessage) = await _autoTaskSchedulerService.AutoTaskNext((Guid)robotId, token.TaskId, token.NextToken);
      var taskDetail = await GenerateTaskDetail(task);
      return new RpcRespond {
        Status = errorMessage == string.Empty ? RpcResultStatus.Success : RpcResultStatus.Failed,
        Message = errorMessage,
        Task = taskDetail
      };
    }

    public override async Task<RpcRespond> AutoTaskAbort(RpcNextToken token, ServerCallContext context)
    {
      var robotId = ValidateRobotClaim(context);
      if (robotId == null)
        return ValidateRobotClaimFailed();

      var result = await _autoTaskSchedulerService.AutoTaskAbort((Guid)robotId, token.TaskId, token.NextToken);
      return new RpcRespond {
        Status = result == string.Empty ? RpcResultStatus.Success : RpcResultStatus.Failed,
        Message = result
      };
    }
  }
}