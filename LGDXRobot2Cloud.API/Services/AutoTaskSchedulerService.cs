using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace LGDXRobot2Cloud.API.Services;

public interface IAutoTaskSchedulerService
{
  Task ClearIgnoreRobot();
  Task<AutoTask?> GetAutoTask(Guid robotId);
  Task<(AutoTask?, string)> AutoTaskAbort(Guid robotId, int taskId, string token);
  Task<(AutoTask?, string)> AutoTaskNext(Guid robotId, int taskId, string token);
}

public class AutoTaskSchedulerService(IAutoTaskRepository autoTaskRepository,
  IProgressRepository progressRepository,
  IDistributedCache cache,
  IOnlineRobotsService onlineRobotsService) : IAutoTaskSchedulerService
{
  private readonly DistributedCacheEntryOptions _cacheEntryOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
  private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
  private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly IProgressRepository _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));

  public async Task ClearIgnoreRobot()
  {
    await _cache.RemoveAsync("AutoTaskSchedulerService_IgnoreRobot");
  }

  public async Task<AutoTask?> GetAutoTask(Guid robotId)
  {
    if (await _onlineRobotsService.GetPauseAutoTaskAssignment(robotId))
      return null;
    
    var ignoreRobotIds = await _cache.GetAsync<HashSet<Guid>>("AutoTaskSchedulerService_IgnoreRobot");
    if (ignoreRobotIds != null && (ignoreRobotIds?.Contains(robotId) ?? false))
    {
      return null;
    }
    
    var currentTask = await _autoTaskRepository.GetRunningAutoTaskAsync(robotId) ?? await _autoTaskRepository.AssignAutoTaskAsync(robotId);
    if (currentTask != null)
    {
      var progress = await _progressRepository.GetProgressAsync(currentTask.CurrentProgressId);
      currentTask.CurrentProgress = progress!;
    }
    else
    {
      // No task for this robot, pause database access.
      ignoreRobotIds ??= [];
      ignoreRobotIds.Add(robotId);
      await _cache.SetAsync("AutoTaskSchedulerService_IgnoreRobot", ignoreRobotIds, _cacheEntryOptions);
    }
    return currentTask;
  }

  public async Task<(AutoTask?, string)> AutoTaskAbort(Guid robotId, int taskId, string token)
  {
    var task = await _autoTaskRepository.AutoTaskAbortAsync(robotId, taskId, token);
    if (task == null)
    {
      return (null, "Task not found / Invalid token.");
    }
    var progress = await _progressRepository.GetProgressAsync(task.CurrentProgressId);
    task.CurrentProgress = progress!;
    return (task, "");
  }

  public async Task<(AutoTask?, string)> AutoTaskNext(Guid robotId, int taskId, string token)
  {
    var task = await _autoTaskRepository.AutoTaskNextAsync(robotId, taskId, token);
    if (task == null)
    {
      return (null, "Task not found / Invalid token.");
    }
    var progress = await _progressRepository.GetProgressAsync(task.CurrentProgressId);
    task.CurrentProgress = progress!;
    return (task, "");
  }
}