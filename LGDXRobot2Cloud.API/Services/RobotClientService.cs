using System.Security.Claims;
using Grpc.Core;
using LGDXRobot2Cloud.Protos;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authorization;
using static LGDXRobot2Cloud.Protos.RobotClientService;

namespace LGDXRobot2Cloud.API.Services
{
  [Authorize(AuthenticationSchemes = CertificateAuthenticationDefaults.AuthenticationScheme)]
  public class RobotClientService(IAutoTaskSchedulerService autoTaskSchedulerService) : RobotClientServiceBase
  {
    private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));

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
      var taskDetail = new TaskProgressDetail();
      if (data.GetTask == true)
      {
        var task = await _autoTaskSchedulerService.GetAutoTask(Guid.Parse(robotId));
        if (task != null) 
        {
          taskDetail = new TaskProgressDetail{
            TaskId = task.Id,
            TaskName = task.Name,
            TaskProgressId = task.CurrentProgressId,
            TaskProgressName = task.CurrentProgress.Name,
            Waypoints = {},
            CompleteToken = task.CompleteToken
          };
        }
      }
      return new ExchangeReturn{
        Result = new ResultMessage {
          Status = ResultStatus.Success,
          Message = ""
        },
        Task = taskDetail
      };
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

      var result = await _autoTaskSchedulerService.CompleteProgress(Guid.Parse(robotId), token.TaskId, token.Token);
      return new ExchangeReturn {
        Result = new ResultMessage {
          Status = result.Item2 == string.Empty ? ResultStatus.Success : ResultStatus.Failed,
          Message = result.Item2
        },
        Task = result.Item1 != null 
        ? new TaskProgressDetail{
          TaskId = result.Item1.Id,
          TaskName = result.Item1.Name,
          TaskProgressId = result.Item1.CurrentProgressId,
          TaskProgressName = result.Item1.CurrentProgress.Name,
          Waypoints = {},
          CompleteToken = result.Item1.CompleteToken
        } 
        : null
      };
    }

    public override async Task<ExchangeReturn> AbortAutoTask(CompleteToken token, ServerCallContext context)
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

      var result = await _autoTaskSchedulerService.AbortAutoTask(Guid.Parse(robotId), token.TaskId, token.Token);
      return new ExchangeReturn {
        Result = new ResultMessage {
          Status = result == string.Empty ? ResultStatus.Success : ResultStatus.Failed,
          Message = result
        }
      };
    }

    public override Task<ResultMessage> UpdateSpecification(RobotSpecification specification, ServerCallContext context)
    {
      return Task.FromResult(new ResultMessage());
    }
  }
}