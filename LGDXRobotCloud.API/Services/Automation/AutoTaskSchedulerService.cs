using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Automation;

public interface IAutoTaskSchedulerService
{
  void ResetIgnoreRobot(int realmId);
  Task<RobotClientsAutoTask?> GetAutoTaskAsync(Guid robotId);
  Task<RobotClientsAutoTask?> AutoTaskAbortAsync(Guid robotId, int taskId, string token, AutoTaskAbortReason autoTaskAbortReason);
  Task<bool> AutoTaskAbortApiAsync(int taskId);
  Task<RobotClientsAutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token);
  Task<AutoTask?> AutoTaskNextApiAsync(Guid robotId, int taskId, string token);
  Task<RobotClientsAutoTask?> AutoTaskNextConstructAsync(AutoTask autoTask);
}

public class AutoTaskSchedulerService(
    IBus bus,
    IEmailService emailService,
    IMemoryCache memoryCache,
    IOnlineRobotsService onlineRobotsService,
    IRobotService robotService,
    ITriggerService triggerService,
    LgdxContext context
  ) : IAutoTaskSchedulerService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private static string GetIgnoreRobotsKey(int realmId) => $"AutoTaskSchedulerService_IgnoreRobot_{realmId}";

  private async Task AddAutoTaskJourney(AutoTask autoTask)
  {
    var autoTaskJourney = new AutoTaskJourney
    {
      AutoTaskId = autoTask.Id,
      CurrentProgressId = autoTask.CurrentProgressId
    };
    await _context.AutoTasksJourney.AddAsync(autoTaskJourney);
    await _context.SaveChangesAsync();
  }

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
      return new RobotClientsDof
      {
        X = taskDetail.CustomX != null ? (double)taskDetail.CustomX : 0,
        Y = taskDetail.CustomY != null ? (double)taskDetail.CustomY : 0,
        Rotation = taskDetail.CustomRotation != null ? (double)taskDetail.CustomRotation : 0
      };
    }
  }

  private async Task<RobotClientsAutoTask?> GenerateTaskDetail(AutoTask? task, bool continueAutoTask = false)
  {
    if (task == null)
    {
      return null;
    }

    var progress = await _context.Progresses.AsNoTracking()
      .Where(p => p.Id == task.CurrentProgressId)
      .Select(p => new { p.Name })
      .FirstOrDefaultAsync();
    if (!continueAutoTask)
    {
      // Notify the updated task
      var flowName = await _context.Flows.AsNoTracking()
        .Where(f => f.Id == task.FlowId)
        .Select(f => f.Name)
        .FirstOrDefaultAsync();
      string? robotName = null;
      if (task.AssignedRobotId != null)
      {
        robotName = await _context.Robots.AsNoTracking()
          .Where(r => r.Id == task.AssignedRobotId)
          .Select(r => r.Name)
          .FirstOrDefaultAsync();
      }
      await _bus.Publish(new AutoTaskUpdateContract{
        Id = task.Id,
        Name = task.Name,
        Priority = task.Priority,
        FlowId = task.FlowId ?? 0,
        FlowName = flowName ?? "Deleted Flow",
        RealmId = task.RealmId,
        AssignedRobotId = task.AssignedRobotId,
        AssignedRobotName = robotName,
        CurrentProgressId = task.CurrentProgressId,
        CurrentProgressName = progress!.Name,
      });
    }
    
    if (task.CurrentProgressId == (int)ProgressState.Completed || task.CurrentProgressId == (int)ProgressState.Aborted)
    {
      // Return immediately if the task is completed / aborted
      return new RobotClientsAutoTask {
        TaskId = task.Id,
        TaskName = task.Name ?? string.Empty,
        TaskProgressId = task.CurrentProgressId,
        TaskProgressName = progress!.Name ?? string.Empty,
        Waypoints = {},
        NextToken = string.Empty,
      };
    }
      
    var flowDetail = await _context.FlowDetails.AsNoTracking()
      .Where(fd => fd.FlowId == task.FlowId && fd.Order == (int)task.CurrentProgressOrder!)
      .FirstOrDefaultAsync();
    if (!continueAutoTask && flowDetail!.TriggerId != null)
    {
      // Fire the trigger
      await _triggerService.InitialiseTriggerAsync(task, flowDetail);
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
      TaskProgressName = progress!.Name ?? string.Empty,
      Waypoints = { waypoints },
      NextToken = nextToken,
    };
  }

  public void ResetIgnoreRobot(int realmId)
  {
    _memoryCache.Remove(GetIgnoreRobotsKey(realmId));
  }

  private async Task<AutoTask?> GetRunningAutoTaskSqlAsync(Guid robotId)
  {
    return await _context.AutoTasks.AsNoTracking()
      .Include(t => t.AutoTaskDetails)
      .Where(t => t.AssignedRobotId == robotId)
      .Where(t => !LgdxHelper.AutoTaskStaticStates.Contains(t.CurrentProgressId))
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
        $@"SELECT * FROM ""Automation.AutoTasks"" AS T 
            WHERE T.""CurrentProgressId"" = {(int)ProgressState.Waiting} AND (T.""AssignedRobotId"" = {robotId} OR T.""AssignedRobotId"" IS NULL)
            ORDER BY T.""Priority"" DESC, T.""AssignedRobotId"" DESC, T.""Id""
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
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    _memoryCache.TryGetValue(GetIgnoreRobotsKey(realmId), out HashSet<Guid>? ignoreRobotIds);
    if (ignoreRobotIds != null && (ignoreRobotIds?.Contains(robotId) ?? false))
    {
      return null;
    }

    var currentTask = await GetRunningAutoTaskSqlAsync(robotId);
    bool continueAutoTask = currentTask != null;
    if (currentTask == null)
    {
      if (!_onlineRobotsService.GetPauseAutoTaskAssignment(robotId))
      {
        currentTask = await AssignAutoTaskSqlAsync(robotId);
        if (currentTask != null)
        {
          await AddAutoTaskJourney(currentTask);
        }
      }
      else
      {
        // If pause auto task assignment is true, new task will not be assigned.
        return null;
      }
    }

    if (currentTask == null)
    {
      // No task for this robot, pause database access.
      ignoreRobotIds ??= [];
      ignoreRobotIds.Add(robotId);
      _memoryCache.Set(GetIgnoreRobotsKey(realmId), ignoreRobotIds, TimeSpan.FromMinutes(5));
    }
    return await GenerateTaskDetail(currentTask, continueAutoTask);
  }

  private async Task<AutoTask?> AutoTaskAbortSqlAsync(int taskId, Guid? robotId = null, string? token = null)
  {
    AutoTask? task = null;
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Get task
      if (robotId == null && string.IsNullOrWhiteSpace(token))
      {
        // From API
        task = await _context.AutoTasks.FromSql(
          $@"SELECT * FROM ""Automation.AutoTasks"" AS T
            WHERE T.""Id"" = {taskId}
            LIMIT 1 FOR UPDATE NOWAIT"
        ).FirstOrDefaultAsync();
      }
      else
      {
        task = await _context.AutoTasks.FromSql(
          $@"SELECT * FROM ""Automation.AutoTasks"" AS T
              WHERE T.""Id"" = {taskId} AND T.""AssignedRobotId"" = {robotId} AND T.""NextToken"" = {token}
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

  private async Task DeleteTriggerRetries(int taskId)
  {
    var count = await _context.TriggerRetries.Where(tr => tr.AutoTaskId == taskId).CountAsync();
    if (count > 0)
    {
      await _context.TriggerRetries.Where(tr => tr.AutoTaskId == taskId).ExecuteDeleteAsync();
    }
  }

  public async Task<RobotClientsAutoTask?> AutoTaskAbortAsync(Guid robotId, int taskId, string token, AutoTaskAbortReason autoTaskAbortReason)
  {
    var task = await AutoTaskAbortSqlAsync(taskId, robotId, token);
    if (task != null)
    {
      await DeleteTriggerRetries(taskId);
      await _emailService.SendAutoTaskAbortEmailAsync(robotId, taskId, autoTaskAbortReason);
      await AddAutoTaskJourney(task);
    }
    return await GenerateTaskDetail(task);
  }

  public async Task<bool> AutoTaskAbortApiAsync(int taskId)
  {
    var task = await AutoTaskAbortSqlAsync(taskId);
    if (task == null)
      return false;
      
    await DeleteTriggerRetries(taskId);
    await _emailService.SendAutoTaskAbortEmailAsync((Guid)task!.AssignedRobotId!, taskId, AutoTaskAbortReason.UserApi);
    await AddAutoTaskJourney(task);
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
        $@"SELECT * FROM ""Automation.AutoTasks"" AS T
            WHERE T.""Id"" = {taskId} AND T.""AssignedRobotId"" = {robotId} AND T.""NextToken"" = {token}
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
      if (flowDetail != null)
      {
        task!.CurrentProgressId = flowDetail!.ProgressId;
        task.CurrentProgressOrder = flowDetail.Order;
        task.NextToken = LgdxHelper.GenerateMd5Hash($"{robotId} {task.Id} {task.CurrentProgressId} {DateTime.UtcNow}");
      }
      else
      {
        task!.CurrentProgressId = (int)ProgressState.Completed;
        task.CurrentProgressOrder = null;
        task.NextToken = null;
      }
      
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
    if (task != null)
    {
      await AddAutoTaskJourney(task);
    }
    return await GenerateTaskDetail(task);
  }

  public async Task<AutoTask?> AutoTaskNextApiAsync(Guid robotId, int taskId, string token)
  {
    var task = await AutoTaskNextSqlAsync(robotId, taskId, token);
    if (task != null)
    {
      await AddAutoTaskJourney(task);
    }
    return task;
  }

  public async Task<RobotClientsAutoTask?> AutoTaskNextConstructAsync(AutoTask autoTask)
  {
    return await GenerateTaskDetail(autoTask);
  }
}