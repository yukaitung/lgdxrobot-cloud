using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.Extensions.Caching.Distributed;

namespace LGDXRobot2Cloud.API.Services;

public interface IAutoTaskSchedulerService
{
  Task ResetIgnoreRobotAsync();
  Task<RobotClientsAutoTask?> GetAutoTaskAsync(Guid robotId);
  Task<RobotClientsAutoTask?> AutoTaskAbortAsync(Guid robotId, int taskId, string token);
  Task<RobotClientsAutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token);
  Task<RobotClientsAutoTask?> AutoTaskNextManualAsync(AutoTask autoTask);
}

public class AutoTaskSchedulerService(
    IAutoTaskDetailRepository autoTaskDetailRepository,
    IAutoTaskRepository autoTaskRepository,
    IDistributedCache cache,
    IFlowDetailRepository flowDetailRepository,
    IOnlineRobotsService onlineRobotsService,
    IProgressRepository progressRepository,
    ITriggerService triggerService
  ) : IAutoTaskSchedulerService
{
  private readonly IAutoTaskDetailRepository _autoTaskDetailRepository = autoTaskDetailRepository ?? throw new ArgumentNullException(nameof(autoTaskDetailRepository));
  private readonly DistributedCacheEntryOptions _cacheEntryOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
  private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
  private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
  private readonly IFlowDetailRepository _flowDetailRepository = flowDetailRepository ?? throw new ArgumentNullException(nameof(flowDetailRepository));
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly IProgressRepository _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));

  private static RobotClientsDof GenerateWaypoint(AutoTaskDetail taskDetail)
  {
    if (taskDetail.Waypoint != null)
    {
      var waypoint = new RobotClientsDof 
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
      return new RobotClientsDof { 
        X = taskDetail.CustomX != null ? (double)taskDetail.CustomX : 0, 
        Y = taskDetail.CustomY != null ? (double)taskDetail.CustomY : 0, 
        Rotation = taskDetail.CustomRotation != null ? (double)taskDetail.CustomRotation : 0 };
    }
  }

  private async Task<RobotClientsAutoTask?> GenerateTaskDetail(AutoTask? task, bool ignoreTrigger = false)
  {
    if (task == null)
    {
      return null;
    }

    if (task.CurrentProgressId == (int)ProgressState.Completed || task.CurrentProgressId == (int)ProgressState.Aborted)
    {
      return new RobotClientsAutoTask {
        TaskId = task.Id,
        TaskName = task.Name ?? string.Empty,
        TaskProgressId = task.CurrentProgressId,
        TaskProgressName = task.CurrentProgress.Name ?? string.Empty,
        Waypoints = {},
        NextToken = string.Empty,
      };
    }
      
    var flowDetail = await _flowDetailRepository.GetFlowDetailAsync(task.FlowId, (int) task.CurrentProgressOrder!);
    if (!ignoreTrigger && flowDetail!.Trigger != null)
    {
      await _triggerService.InitiateTriggerAsync(task, flowDetail);
    }

    List<RobotClientsDof> waypoints = [];
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

    string nextToken = task.NextToken ?? string.Empty;
    if (flowDetail?.AutoTaskNextControllerId != (int) AutoTaskNextController.Robot)
    {
      // API has the control
      nextToken = string.Empty;
    }

    return new RobotClientsAutoTask {
      TaskId = task.Id,
      TaskName = task.Name ?? string.Empty,
      TaskProgressId = task.CurrentProgressId,
      TaskProgressName = task.CurrentProgress.Name ?? string.Empty,
      Waypoints = { waypoints },
      NextToken = nextToken,
    };
  }

  public async Task ResetIgnoreRobotAsync()
  {
    await _cache.RemoveAsync("AutoTaskSchedulerService_IgnoreRobot");
  }

  public async Task<RobotClientsAutoTask?> GetAutoTaskAsync(Guid robotId)
  {
    if (await _onlineRobotsService.GetPauseAutoTaskAssignmentAsync(robotId))
      return null;
    
    var ignoreRobotIds = await _cache.GetAsync<HashSet<Guid>>("AutoTaskSchedulerService_IgnoreRobot");
    if (ignoreRobotIds != null && (ignoreRobotIds?.Contains(robotId) ?? false))
    {
      return null;
    }

    var currentTask = await _autoTaskRepository.GetRunningAutoTaskAsync(robotId);
    var ignoreTrigger = currentTask != null; // Do not fire trigger if there is a task running
    currentTask ??= await _autoTaskRepository.AssignAutoTaskAsync(robotId);
    
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
    return await GenerateTaskDetail(currentTask, ignoreTrigger);
  }

  public async Task<RobotClientsAutoTask?> AutoTaskAbortAsync(Guid robotId, int taskId, string token)
  {
    var task = await _autoTaskRepository.AutoTaskAbortAsync(robotId, taskId, token);
    if (task == null)
    {
      return null;
    }
    var progress = await _progressRepository.GetProgressAsync(task.CurrentProgressId);
    task.CurrentProgress = progress!;
    return await GenerateTaskDetail(task);
  }

  public async Task<RobotClientsAutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token)
  {
    var task = await _autoTaskRepository.AutoTaskNextAsync(robotId, taskId, token);
    if (task == null)
    {
      return null;
    }
    var progress = await _progressRepository.GetProgressAsync(task.CurrentProgressId);
    task.CurrentProgress = progress!;
    return await GenerateTaskDetail(task, true);
  }

  public async Task<RobotClientsAutoTask?> AutoTaskNextManualAsync(AutoTask autoTask)
  {
    var progress = await _progressRepository.GetProgressAsync(autoTask.CurrentProgressId);
    autoTask.CurrentProgress = progress!;
    return await GenerateTaskDetail(autoTask, true);
  }
}