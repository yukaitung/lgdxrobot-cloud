using Grpc.Core;
using LGDXRobot2Cloud.Protos;
using static LGDXRobot2Cloud.Protos.RobotClientService;

namespace LGDXRobot2Cloud.Services
{
  public class RobotClientService(IAutoTaskSchedulerService autoTaskSchedulerService) : RobotClientServiceBase
  {
    private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
    
    public override async Task<ExchangeReturn> Exchange(RobotData data, ServerCallContext context)
    {
      // Set Robot Data

      // Get AutoTask
      var taskDetail = new TaskProgressDetail();
      if (data.GetTask == true)
      {
        var task = await _autoTaskSchedulerService.GetAutoTask(1);
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
      if (token.Abort == true)
      {
        var result = await _autoTaskSchedulerService.AbortAutoTask(1, token.TaskId, token.Token);
        return new ExchangeReturn {
          Result = new ResultMessage {
            Status = result == string.Empty ? ResultStatus.Success : ResultStatus.Failed,
            Message = result
          }
        };
      }
      else
      {
        var result = await _autoTaskSchedulerService.CompleteProgress(1, token.TaskId, token.Token);
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
            Waypoints = {},
            CompleteToken = result.Item1.CompleteToken
          } 
          : null
        };
      }
    }

    public override Task<ResultMessage> UpdateSpecification(RobotSpecification specification, ServerCallContext context)
    {
      Console.WriteLine("Task<StatusMessage> UpdateRobotSpecification(RobotSpecification specification, ServerCallContext context)");
      return Task.FromResult(new ResultMessage());
    }
  }
}