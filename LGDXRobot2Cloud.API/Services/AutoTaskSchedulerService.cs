using System.Security.Cryptography;
using System.Text;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Utilities;

namespace LGDXRobot2Cloud.Services
{
  public class AutoTaskSchedulerService(IAutoTaskRepository autoTaskRepository,
    IFlowRepository flowRepository) : IAutoTaskSchedulerService
  {
    private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
    private readonly IFlowRepository _flowRepository = flowRepository ?? throw new ArgumentNullException(nameof(flowRepository));
    
    private async Task<AutoTask?> AssignAutoTask(int robotId)
    {
      var task = await _autoTaskRepository.GetFirstWaitingAutoTaskAsync(robotId);
      if (task == null)
        return null;
      var flow = await _flowRepository.GetFlowProgressesAsync(task.FlowId);
      if (flow == null)
        return null;
      var nextProgress = flow.FlowDetails[0].Progress;
      var completeToken = GenerateCompleteToken(robotId, task.Id, nextProgress.Id);
      
      task.AssignedRobotId = robotId;
      task.CurrentProgress = nextProgress;
      task.CompleteToken = completeToken;
      if (await _autoTaskRepository.SaveChangesAsync())
        return task;
      else
        return null;
    }

    public async Task<AutoTask?> GetAutoTask(int robotId)
    {
      var currentTask = await _autoTaskRepository.GetOnGoingAutoTaskAsync(robotId);
      if (currentTask == null) 
      {
        currentTask = await AssignAutoTask(robotId);
      }
      return currentTask;
    }

    public async Task<string> AbortAutoTask(int robotId, int taskId, string token)
    {
      var task = await _autoTaskRepository.GetAutoTaskToComplete(robotId, taskId, token);
      if (task == null)
        return "Task not found / Invalid token.";
      task.CurrentProgressId = (int)ProgressState.Aborted;
      task.CompleteToken = null;
      if (await _autoTaskRepository.SaveChangesAsync())
        return string.Empty;
      else
        return "Database error.";
    }

    private static string GenerateCompleteToken(int robotId, int taskId, int progressId)
    {
      string str = robotId.ToString() + " " + taskId.ToString() + " " + progressId.ToString() + " " + DateTime.UtcNow.ToFileTime().ToString();
      var bytes = Encoding.UTF8.GetBytes(str);
      var hash = MD5.HashData(bytes);
      return Convert.ToHexString(hash);
    }

    public async Task<(AutoTask?, string)> CompleteProgress(int robotId, int taskId, string token)
    {
      var task = await _autoTaskRepository.GetAutoTaskToComplete(robotId, taskId, token);
      if (task == null)
        return (null, "Task not found / Invalid token.");
      var flow = await _flowRepository.GetFlowProgressesAsync(task.FlowId);
      if (flow == null)
        return (null, "Flow not found.");
      for (int i = 0; i < flow.FlowDetails.Count; i++)
      {
        if (flow.FlowDetails[i].ProgressId == task.CurrentProgressId)
        {
          if (i == flow.FlowDetails.Count - 1)
          {
            // Complete
            task.CurrentProgressId = (int)ProgressState.Completed;
            task.CompleteToken = null;
            if (await _autoTaskRepository.SaveChangesAsync())
              return (null, string.Empty);
            else
              return (null, "Database error.");
          }
          else
          {
            // Set next progress
            var nextProgress = flow.FlowDetails[i + 1].Progress;
            var completeToken = GenerateCompleteToken(robotId, task.Id, nextProgress.Id);

            task.CurrentProgress = nextProgress;
            task.CompleteToken = completeToken;
            if (await _autoTaskRepository.SaveChangesAsync())
              return (task, string.Empty);
            else
              return (null, "Database error.");
          }
        }
      }
      return (null, "Invalid progress.");
    }
  }
}