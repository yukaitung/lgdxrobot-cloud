using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Enums;
using LGDXRobot2Cloud.Shared.Utilities;

namespace LGDXRobot2Cloud.API.Services
{
  public interface IAutoTaskSchedulerService
  {
    Task<AutoTask?> GetAutoTask(Guid robotId);
    Task<string> AutoTaskAbort(Guid robotId, int taskId, string token);
    Task<(AutoTask?, string)> AutoTaskNext(Guid robotId, int taskId, string token);
  }
  
  public class AutoTaskSchedulerService(IAutoTaskRepository autoTaskRepository,
    IProgressRepository progressRepository) : IAutoTaskSchedulerService
  {
    private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
    private readonly IProgressRepository _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
    
    public async Task<AutoTask?> GetAutoTask(Guid robotId)
    {
      var currentTask = await _autoTaskRepository.GetRunningAutoTaskAsync(robotId) ?? await _autoTaskRepository.AssignAutoTaskAsync(robotId);
      if (currentTask != null)
      {
        var progress = await _progressRepository.GetProgressAsync(currentTask.CurrentProgressId);
        currentTask.CurrentProgress = progress!;
      }
      return currentTask;
    }

    public async Task<string> AutoTaskAbort(Guid robotId, int taskId, string token)
    {
      var task = await _autoTaskRepository.AutoTaskAbortAsync(robotId, taskId, token);
      if (task == null)
        return "Task not found / Invalid token.";
      if (task.CurrentProgressId == (int)ProgressState.Aborted)
        return "";
      else
        return "Database Error.";
    }

    public async Task<(AutoTask?, string)> AutoTaskNext(Guid robotId, int taskId, string token)
    {
      var task = await _autoTaskRepository.AutoTaskNextAsync(robotId, taskId, token);
      if (task == null)
        return (null, "Task not found / Invalid token.");
      if (task.CurrentProgressId == (int)ProgressState.Completed)
      {
        return (null, "");
      }
      else
      {
        var progress = await _progressRepository.GetProgressAsync(task.CurrentProgressId);
        task.CurrentProgress = progress!;
        return (task, "");
      }
    }
  }
}