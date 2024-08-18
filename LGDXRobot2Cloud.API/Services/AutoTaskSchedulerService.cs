using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.API.Services
{
  public interface IAutoTaskSchedulerService
  {
    void ClearIgnoreRobot();
    Task<AutoTask?> GetAutoTask(Guid robotId);
    Task<string> AutoTaskAbort(Guid robotId, int taskId, string token);
    Task<(AutoTask?, string)> AutoTaskNext(Guid robotId, int taskId, string token);
  }
  
  public class AutoTaskSchedulerService(IAutoTaskRepository autoTaskRepository,
    IProgressRepository progressRepository,
    IMemoryCache memoryCache) : IAutoTaskSchedulerService
  {
    private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
    private readonly IProgressRepository _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    
    public void ClearIgnoreRobot()
    {
      _memoryCache.Remove("AutoTaskSchedulerService_IgnoreRobot");
    }

    public async Task<AutoTask?> GetAutoTask(Guid robotId)
    {
      _memoryCache.TryGetValue<HashSet<Guid>>("AutoTaskSchedulerService_IgnoreRobot", out var ignoreRobotIds);
      if (ignoreRobotIds?.Contains(robotId) ?? false)
        return null;
      var currentTask = await _autoTaskRepository.GetRunningAutoTaskAsync(robotId) ?? await _autoTaskRepository.AssignAutoTaskAsync(robotId);
      if (currentTask != null)
      {
        var progress = await _progressRepository.GetProgressAsync(currentTask.CurrentProgressId);
        currentTask.CurrentProgress = progress!;
      }
      else
      {
        ignoreRobotIds ??= [];
        ignoreRobotIds.Add(robotId);
        _memoryCache.Set("AutoTaskSchedulerService_IgnoreRobot", ignoreRobotIds, TimeSpan.FromMinutes(5));
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