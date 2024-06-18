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
  IAutoTaskDetailRepository autoTaskDetailRepository) : RobotClientServiceBase
  {
    private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
    private readonly IAutoTaskDetailRepository _autoTaskDetailRepository = autoTaskDetailRepository ?? throw new ArgumentNullException(nameof(autoTaskDetailRepository));

    private async Task<TaskProgressDetail?> GenerateTaskDetail(AutoTask? task)
    {
      if (task == null)
        return null;
      List<Dof> waypoints = [];
      if (task.CurrentProgressId == (int)ProgressState.Moving)
      {
        var taskDetails = await _autoTaskDetailRepository.GetAutoTaskDetailsAsync(task.Id);
        foreach (var t in taskDetails)
        {
          waypoints.Add(new Dof {X = t.Waypoint.X, Y = t.Waypoint.Y, W = t.Waypoint.W});
        }
      }
      return new TaskProgressDetail{
        TaskId = task.Id,
        TaskName = task.Name,
        TaskProgressId = task.CurrentProgressId,
        TaskProgressName = task.CurrentProgress.Name,
        Waypoints = {waypoints},
        CompleteToken = task.CompleteToken
      };
    }

    public override async Task<ExchangeReturn> Exchange(RobotData data, ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return new ExchangeReturn{
          Result = new ResultMessage {
            Status = ResultStatus.Failed,
            Message = "Robot ID is missing in CN."
          }
        };
      }
      var robotId = robotClaim.Value;
      // TODO: Set Robot Data

      // Get AutoTask
      if (data.GetTask == true)
      {
        var task = await _autoTaskSchedulerService.GetAutoTask(Guid.Parse(robotId));
        var taskDetail = await GenerateTaskDetail(task);
        return new ExchangeReturn{
          Result = new ResultMessage {
            Status = ResultStatus.Success,
            Message = ""
          },
          Task = taskDetail
        };
      }
      else
      {
        return new ExchangeReturn{
          Result = new ResultMessage {
            Status = ResultStatus.Success,
            Message = ""
          }
        };
      }
    }

    public override async Task<ExchangeReturn> CompleteProgress(CompleteToken token, ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return new ExchangeReturn{
          Result = new ResultMessage {
            Status = ResultStatus.Failed,
            Message = "Robot ID is missing in CN."
          }
        };
      }
      var robotId = robotClaim.Value;

      var (task, errorMessage) = await _autoTaskSchedulerService.CompleteProgress(Guid.Parse(robotId), token.TaskId, token.Token);
      var taskDetail = await GenerateTaskDetail(task);
      return new ExchangeReturn {
        Result = new ResultMessage {
          Status = errorMessage == string.Empty ? ResultStatus.Success : ResultStatus.Failed,
          Message = errorMessage
        },
        Task = taskDetail
      };
    }

    public override async Task<ResultMessage> AbortAutoTask(CompleteToken token, ServerCallContext context)
    {
      var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
      if (robotClaim == null)
      {
        return new ResultMessage {
          Status = ResultStatus.Failed,
          Message = "Robot ID is missing in CN."
        };
      }
      var robotId = robotClaim.Value;

      var result = await _autoTaskSchedulerService.AbortAutoTask(Guid.Parse(robotId), token.TaskId, token.Token);
      return  new ResultMessage {
        Status = result == string.Empty ? ResultStatus.Success : ResultStatus.Failed,
        Message = result
      };
    }

    public override Task<ResultMessage> UpdateSpecification(RobotSpecification specification, ServerCallContext context)
    {
      return Task.FromResult(new ResultMessage());
    }
  }
}