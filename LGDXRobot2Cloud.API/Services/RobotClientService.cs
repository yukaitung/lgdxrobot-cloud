using System.Reflection.Metadata;
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
    private readonly IRobotDataService _robotDataService = robotDataService ?? throw new ArgumentNullException(nameof(robotDataService));
    private readonly IAutoTaskDetailRepository _autoTaskDetailRepository = autoTaskDetailRepository ?? throw new ArgumentNullException(nameof(autoTaskDetailRepository));
    private readonly IRobotSystemInfoRepository _robotSystemInfoRepository = robotSystemInfoRepository ?? throw new ArgumentNullException(nameof(robotSystemInfoRepository));

    private async Task<RpcTaskProgressDetail?> GenerateTaskDetail(AutoTask? task)
    {
      if (task == null)
        return null;
      List<RpcRobotDof> waypoints = [];
      if (task.CurrentProgressId == (int)ProgressState.Moving)
      {
        var taskDetails = await _autoTaskDetailRepository.GetAutoTaskDetailsAsync(task.Id);
        foreach (var t in taskDetails)
        {
          waypoints.Add(new RpcRobotDof {X = t.Waypoint.X, Y = t.Waypoint.Y, W = t.Waypoint.W});
        }
      }
      return new RpcTaskProgressDetail{
        TaskId = task.Id,
        TaskName = task.Name,
        TaskProgressId = task.CurrentProgressId,
        TaskProgressName = task.CurrentProgress.Name,
        Waypoints = {waypoints},
        CompleteToken = task.CompleteToken
      };
    }

    static private RobotData GenerateRobotData(RpcRobotExchangeData data)
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
        Velocity = (data.Velocity.X, data.Velocity.Y, data.Velocity.W),
        EmergencyStopsEnabled = emergencyStopsEnabled
      };
    }

    public override async Task<RpcResultMessageWithTask> Exchange(RpcRobotExchangeData data, ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return new RpcResultMessageWithTask{
          Result = new RpcResultMessage {
            Status = RpcResultStatus.Failed,
            Message = "Robot ID is missing in CN."
          }
        };
      }
      var robotId = Guid.Parse(robotClaim.Value);

      _robotDataService.SetRobotData(robotId, GenerateRobotData(data));
      // Get AutoTask
      if (data.GetTask == true)
      {
        var task = await _autoTaskSchedulerService.GetAutoTask(robotId);
        var taskDetail = await GenerateTaskDetail(task);
        return new RpcResultMessageWithTask{
          Result = new RpcResultMessage {
            Status = RpcResultStatus.Success,
            Message = ""
          },
          Task = taskDetail
        };
      }
      else
      {
        return new RpcResultMessageWithTask{
          Result = new RpcResultMessage {
            Status = RpcResultStatus.Success,
            Message = ""
          }
        };
      }
    }

    public override async Task<RpcResultMessageWithTask> CompleteProgress(RpcCompleteToken token, ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return new RpcResultMessageWithTask{
          Result = new RpcResultMessage {
            Status = RpcResultStatus.Failed,
            Message = "Robot ID is missing in CN."
          }
        };
      }
      var robotId = Guid.Parse(robotClaim.Value);

      var (task, errorMessage) = await _autoTaskSchedulerService.CompleteProgress(robotId, token.TaskId, token.Token);
      var taskDetail = await GenerateTaskDetail(task);
      return new RpcResultMessageWithTask {
        Result = new RpcResultMessage {
          Status = errorMessage == string.Empty ? RpcResultStatus.Success : RpcResultStatus.Failed,
          Message = errorMessage
        },
        Task = taskDetail
      };
    }

    public override async Task<RpcResultMessage> AbortAutoTask(RpcCompleteToken token, ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return new RpcResultMessage {
          Status = RpcResultStatus.Failed,
          Message = "Robot ID is missing in CN."
        };
      }
      var robotId = Guid.Parse(robotClaim.Value);

      var result = await _autoTaskSchedulerService.AbortAutoTask(robotId, token.TaskId, token.Token);
      return  new RpcResultMessage {
        Status = result == string.Empty ? RpcResultStatus.Success : RpcResultStatus.Failed,
        Message = result
      };
    }

    public override async Task<RpcResultMessage> UpdateSystemInfo(RpcRobotSystemInfo specification, ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return new RpcResultMessage {
          Status = RpcResultStatus.Failed,
          Message = "Robot ID is missing in CN."
        };
      }
      var robotId = robotClaim.Value;

      bool success;
      var specificationEntity = await _robotSystemInfoRepository.GetRobotSystemInfoAsync(Guid.Parse(robotId));
      if (specificationEntity == null)
      {
        var newSpecificationEntity = new RobotSystemInfo {
          Cpu = specification.Cpu,
          IsLittleEndian = specification.IsLittleEndian,
          RamMiB = specification.RamMiB,
          Gpu = specification.Gpu,
          Os = specification.Os,
          Is32Bit = specification.Is32Bit,
          RobotId = Guid.Parse(robotId)
        };
        await _robotSystemInfoRepository.AddRobotSystemInfoAsync(newSpecificationEntity);
      }
      else
      {
        specificationEntity.Cpu = specification.Cpu;
        specificationEntity.IsLittleEndian = specification.IsLittleEndian;
        specificationEntity.RamMiB = specification.RamMiB;
        specificationEntity.Gpu = specification.Gpu;
        specificationEntity.Os = specification.Os;
        specificationEntity.Is32Bit = specification.Is32Bit;
      }

      success = await _robotSystemInfoRepository.SaveChangesAsync();
      return new RpcResultMessage {
        Status = success ? RpcResultStatus.Success : RpcResultStatus.Failed,
        Message = success ? "" : "Database error."
      };
    }
  }
}