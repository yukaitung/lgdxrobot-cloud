using LGDXRobot2Cloud.API.Extensions;
using LGDXRobot2Cloud.API.Services.Automation;
using LGDXRobot2Cloud.API.Services.Common;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LGDXRobot2Cloud.API.Services.Navigation;

public interface IAutoTaskSchedulerService
{
  Task ResetIgnoreRobotAsync();
  Task<RobotClientsAutoTask?> GetAutoTaskAsync(Guid robotId);
  Task<RobotClientsAutoTask?> AutoTaskAbortAsync(Guid robotId, int taskId, string token, AutoTaskAbortReason autoTaskAbortReason);
  Task<bool> AutoTaskAbortApiAsync(int taskId);
  Task<RobotClientsAutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token);
  Task<AutoTask?> AutoTaskNextApiAsync(Guid robotId, int taskId, string token);
  Task<RobotClientsAutoTask?> AutoTaskNextConstructAsync(AutoTask autoTask);
}

public class AutoTaskSchedulerMySQLService(
    LgdxContext context,
    IDistributedCache cache,
    IOnlineRobotsService onlineRobotsService,
    ITriggerService triggerService,
    IEmailService emailService
  ) : IAutoTaskSchedulerService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly DistributedCacheEntryOptions _cacheEntryOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
  private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

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
      
    var flowDetail = await _context.FlowDetails.AsNoTracking()
      .Where(fd => fd.FlowId == task.FlowId && fd.Order == (int)task.CurrentProgressOrder!)
      .Include(f => f.Trigger)
      .FirstOrDefaultAsync();
    if (!ignoreTrigger && flowDetail!.Trigger != null)
    {
      await _triggerService.InitialiseTriggerAsync(task, flowDetail, flowDetail.Trigger);
    }

    List<RobotClientsDof> waypoints = [];
    if (task.CurrentProgressId == (int)ProgressState.PreMoving)
    {
      var firstTaskDetail = await _context.AutoTasksDetail.AsNoTracking()
        .Where(t => t.AutoTaskId == task.Id)
        .Include(t => t.Waypoint)
        .OrderBy(t => t.Order)
        .FirstOrDefaultAsync();
      if (firstTaskDetail != null)
        waypoints.Add(GenerateWaypoint(firstTaskDetail));
    }
    else if (task.CurrentProgressId == (int)ProgressState.Moving)
    {
      var taskDetails = await _context.AutoTasksDetail.AsNoTracking()
        .Where(t => t.AutoTaskId == task.Id)
        .Include(t => t.Waypoint)
        .OrderBy(t => t.Order)
        .ToListAsync();
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

    var progress = await _context.Progresses.AsNoTracking()
      .Where(p => p.Id == task.CurrentProgressId)
      .Select(p => new { p.Name })
      .FirstOrDefaultAsync();

    return new RobotClientsAutoTask {
      TaskId = task.Id,
      TaskName = task.Name ?? string.Empty,
      TaskProgressId = task.CurrentProgressId,
      TaskProgressName = progress!.Name ?? string.Empty,
      Waypoints = { waypoints },
      NextToken = nextToken,
    };
  }

  public async Task ResetIgnoreRobotAsync()
  {
    await _cache.RemoveAsync("AutoTaskSchedulerService_IgnoreRobot");
  }

  private async Task<AutoTask?> GetRunningAutoTaskSqlAsync(Guid robotId)
  {
    return await _context.AutoTasks.AsNoTracking()
      .Include(t => t.AutoTaskDetails)
      .Where(t => t.AssignedRobotId == robotId)
      .Where(t => !LgdxHelper.AutoTaskRunningStateList.Contains(t.CurrentProgressId))
      .OrderByDescending(t => t.Priority) // In case the robot has multiple running task by mistake 
      .ThenByDescending(t => t.AssignedRobotId)
      .ThenBy(t => t.Id)
      .FirstOrDefaultAsync();
  }

  private async Task<AutoTask?> AssignAutoTaskSqlAsync(Guid robotId)
  {
    AutoTask? task = null;
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Get waiting task
      task = await _context.AutoTasks.FromSql(
        $@"SELECT * FROM `Automation.AutoTasks` AS T 
            WHERE T.`CurrentProgressId` = {(int)ProgressState.Waiting} AND (T.`AssignedRobotId` = {robotId} OR T.`AssignedRobotId` IS NULL)
            ORDER BY T.`Priority` DESC, T.`AssignedRobotId` DESC, T.`Id`
            LIMIT 1 FOR UPDATE SKIP LOCKED"
      ).FirstOrDefaultAsync();

      // Get flow detail
      var flowDetail = await _context.FlowDetails
        .Where(f => f.FlowId == task!.FlowId)
        .Select(f => new { 
          f.ProgressId, 
          f.Order 
        })
        .OrderBy(f => f.Order)
        .FirstOrDefaultAsync();

      // Update task
      task!.AssignedRobotId = robotId;
      task.CurrentProgressId = flowDetail!.ProgressId;
      task.CurrentProgressOrder = flowDetail.Order;
      task.NextToken = LgdxHelper.GenerateMd5Hash($"{robotId} {task.Id} {task.CurrentProgressId} {DateTime.UtcNow}");
      await _context.SaveChangesAsync();
      await transaction.CommitAsync();
      return task;
    }
    catch (Exception)
    {
      await transaction.RollbackAsync();
    }
    return null;
  }

  public async Task<RobotClientsAutoTask?> GetAutoTaskAsync(Guid robotId)
  {
    if (_onlineRobotsService.GetPauseAutoTaskAssignment(robotId))
      return null;
    
    var ignoreRobotIds = await _cache.GetAsync<HashSet<Guid>>("AutoTaskSchedulerService_IgnoreRobot");
    if (ignoreRobotIds != null && (ignoreRobotIds?.Contains(robotId) ?? false))
    {
      return null;
    }

    var currentTask = await GetRunningAutoTaskSqlAsync(robotId);
    var ignoreTrigger = currentTask != null; // Do not fire trigger if there is a task running
    currentTask ??= await AssignAutoTaskSqlAsync(robotId);
    
    if (currentTask == null)
    {
      // No task for this robot, pause database access.
      ignoreRobotIds ??= [];
      ignoreRobotIds.Add(robotId);
      await _cache.SetAsync("AutoTaskSchedulerService_IgnoreRobot", ignoreRobotIds, _cacheEntryOptions);
    }
    return await GenerateTaskDetail(currentTask, ignoreTrigger);
  }

  private async Task<AutoTask?> AutoTaskAbortSqlAsync(int taskId, Guid? robotId = null, string? token = null)
  {
    AutoTask? task = null;
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Get task
      if (robotId == null && token == null)
      {
        task = await _context.AutoTasks.FromSql(
          $@"SELECT * FROM `Automation.AutoTasks` AS T
            WHERE T.`Id` = {taskId}
            LIMIT 1 FOR UPDATE NOWAIT"
        ).FirstOrDefaultAsync();
      }
      else
      {
        task = await _context.AutoTasks.FromSql(
          $@"SELECT * FROM `Automation.AutoTasks` AS T
              WHERE T.`Id` = {taskId} AND T.`AssignedRobotId` = {robotId} AND T.`NextToken` = {token}
              LIMIT 1 FOR UPDATE NOWAIT"
        ).FirstOrDefaultAsync();
      }

      // Update task
      task!.CurrentProgressId = (int)ProgressState.Aborted;
      task.CurrentProgressOrder = null;
      task.NextToken = null;
      await _context.SaveChangesAsync();
      await transaction.CommitAsync();
    }
    catch (Exception)
    {
      await transaction.RollbackAsync();
    }
    return task;
  }

  public async Task<RobotClientsAutoTask?> AutoTaskAbortAsync(Guid robotId, int taskId, string token, AutoTaskAbortReason autoTaskAbortReason)
  {
    var task = await AutoTaskAbortSqlAsync(taskId, robotId, token);
    await _emailService.SendAutoTaskAbortEmailAsync(robotId, taskId, autoTaskAbortReason);
    return await GenerateTaskDetail(task);
  }

  public async Task<bool> AutoTaskAbortApiAsync(int taskId)
  {
    var task = await AutoTaskAbortSqlAsync(taskId);
    if (task == null)
      return false;

    await _emailService.SendAutoTaskAbortEmailAsync((Guid)task!.AssignedRobotId!, taskId, AutoTaskAbortReason.UserApi);
    return true;
  }

  private async Task<AutoTask?> AutoTaskNextSqlAsync(Guid robotId, int taskId, string token)
  {
    AutoTask? task = null;
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Get waiting task
      task = await _context.AutoTasks.FromSql(
        $@"SELECT * FROM `Automation.AutoTasks` AS T
            WHERE T.`Id` = {taskId} AND T.`AssignedRobotId` = {robotId} AND T.`NextToken` = {token}
            LIMIT 1 FOR UPDATE NOWAIT"
      ).FirstOrDefaultAsync();

      // Get flow detail
      var flowDetail = await _context.FlowDetails.AsNoTracking()
        .Where(f => f.FlowId == task!.FlowId)
        .Where(f => f.Order > task!.CurrentProgressOrder)
        .Select(f => new { 
          f.ProgressId, 
          f.Order 
        })
        .OrderBy(f => f.Order)
        .FirstOrDefaultAsync();

      // Update task
      task!.AssignedRobotId = robotId;
      task.CurrentProgressId = flowDetail!.ProgressId;
      task.CurrentProgressOrder = flowDetail.Order;
      task.NextToken = LgdxHelper.GenerateMd5Hash($"{robotId} {task.Id} {task.CurrentProgressId} {DateTime.UtcNow}");
      await _context.SaveChangesAsync();
      await transaction.CommitAsync();
    }
    catch (Exception)
    {
      await transaction.RollbackAsync();
    }
    return task;
  }

  public async Task<RobotClientsAutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token)
  {
    var task = await AutoTaskNextSqlAsync(robotId, taskId, token);
    return await GenerateTaskDetail(task, true);
  }

  public async Task<AutoTask?> AutoTaskNextApiAsync(Guid robotId, int taskId, string token)
  {
    return await AutoTaskNextSqlAsync(robotId, taskId, token);
  }

  public async Task<RobotClientsAutoTask?> AutoTaskNextConstructAsync(AutoTask autoTask)
  {
    return await GenerateTaskDetail(autoTask, true);
  }
}