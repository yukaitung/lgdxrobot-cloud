using LGDXRobotCloud.API.Repositories;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Automation;

public interface IAutoTaskSchedulerService
{
  Task RunSchedulerNewAutoTaskAsync(int realmId, Guid? robotId);
  Task RunSchedulerRobotNewJoinAsync(Guid robotId);
  Task<bool> RunSchedulerRobotReadyAsync(Guid robotId);

  Task ReleaseRobotAsync(Guid robotId);

  Task AutoTaskAbortAsync(Guid robotId, int taskId, string token, AutoTaskAbortReason autoTaskAbortReason);
  Task<bool> AutoTaskAbortApiAsync(int taskId);
  Task AutoTaskNextAsync(Guid robotId, int taskId, string token);
  Task<bool> AutoTaskNextApiAsync(Guid robotId, int taskId, string token);
}

public class AutoTaskSchedulerService(
    IAutoTaskPathPlannerService autoTaskPathPlanner,
    IAutoTaskRepository autoTaskRepository,
    IEmailService emailService,
    IOnlineRobotsService onlineRobotsService,
    IRobotService robotService,
    ITriggerService triggerService,
    LgdxContext context
  ) : IAutoTaskSchedulerService
{
  private readonly IAutoTaskPathPlannerService _autoTaskPathPlanner = autoTaskPathPlanner;
  private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository;
  private readonly IEmailService _emailService = emailService;
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService;
  private readonly IRobotService _robotService = robotService;
  private readonly ITriggerService _triggerService = triggerService;
  private readonly LgdxContext _context = context;

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

  private async Task<RobotClientsAutoTask?> GenerateTaskDetail(AutoTask task, bool continueAutoTask = false)
  {
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

      // Send the updated to the Redis queue
      var data = new AutoTaskUpdate
      {
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
      };
      await _autoTaskRepository.AutoTaskHasUpdateAsync(task.RealmId, data);
    }

    if (task.CurrentProgressId == (int)ProgressState.Completed || task.CurrentProgressId == (int)ProgressState.Aborted)
    {
      // Return immediately if the task is completed / aborted
      return new RobotClientsAutoTask
      {
        TaskId = task.Id,
        TaskName = task.Name ?? string.Empty,
        TaskProgressId = task.CurrentProgressId,
        TaskProgressName = progress!.Name ?? string.Empty,
        Paths = { },
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

    List<RobotClientsPath> paths = [];
    try
    {
      paths = await _autoTaskPathPlanner.GeneratePath(task);
    }
    catch (Exception)
    {
      await AutoTaskAbortSqlAsync(task.Id);
      await _emailService.SendAutoTaskAbortEmailAsync(task.Id, AutoTaskAbortReason.PathPlanner);
      await AddAutoTaskJourney(task);
      return null;
    }

    string nextToken = task.NextToken ?? string.Empty;
    if (flowDetail?.AutoTaskNextControllerId != (int)AutoTaskNextController.Robot)
    {
      // API has the control
      nextToken = string.Empty;
    }

    return new RobotClientsAutoTask
    {
      TaskId = task.Id,
      TaskName = task.Name ?? string.Empty,
      TaskProgressId = task.CurrentProgressId,
      TaskProgressName = progress!.Name ?? string.Empty,
      Paths = { paths },
      NextToken = nextToken,
    };
  }

  public async Task RunSchedulerNewAutoTaskAsync(int realmId, Guid? robotId)
  {
    // Find any robot that is idle
    if (robotId == null)
    {
      robotId = await _autoTaskRepository.SchedulerHoldAnyRobotAsync(realmId);
      if (robotId != null)
      {
        await RunSchedulerRobotReadyAsync(robotId.Value);
        // Release the robot when it obtains the task
      }
    }
    else
    {
      if (await _autoTaskRepository.SchedulerHoldRobotAsync(realmId, robotId.Value))
      {
        await RunSchedulerRobotReadyAsync(robotId.Value);
        // Release the robot when it obtains the task
      }
    }
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

  public async Task RunSchedulerRobotNewJoinAsync(Guid robotId)
  {
    if (await _onlineRobotsService.GetPauseAutoTaskAssignmentAsync(robotId))
    {
      return;
    }

    var runningAutoTask = await GetRunningAutoTaskSqlAsync(robotId);
    if (runningAutoTask != null)
    {
      // Send the running task to the robot
      var task = await GenerateTaskDetail(runningAutoTask, true);
      if (task != null)
      {
        var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
        await _autoTaskRepository.AddAutoTaskAsync(realmId, robotId, task);
      }
      return;
    }
    // Assign the task to the robot
    await RunSchedulerRobotReadyAsync(robotId);
  }

  private async Task<AutoTask?> GetNextAutoTaskSqlAsync(Guid robotId, int realmId)
  {
    AutoTask? task = null;
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Get waiting task
      task = await _context.AutoTasks.FromSql(
        $@"SELECT * FROM ""Automation.AutoTasks"" AS T 
            WHERE T.""CurrentProgressId"" = {(int)ProgressState.Waiting} AND (T.""AssignedRobotId"" = {robotId} OR T.""AssignedRobotId"" IS NULL) AND T.""RealmId"" = {realmId}
            ORDER BY T.""Priority"" DESC, T.""AssignedRobotId"" DESC, T.""Id""
            LIMIT 1 FOR UPDATE SKIP LOCKED"
      ).FirstOrDefaultAsync();
      if (task == null)
      {
        return null;
      }

      // Get flow detail
      var flowDetail = await _context.FlowDetails
        .Where(f => f.FlowId == task!.FlowId)
        .Select(f => new
        {
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

  public async Task<bool> RunSchedulerRobotReadyAsync(Guid robotId)
  {
    if (await _onlineRobotsService.GetPauseAutoTaskAssignmentAsync(robotId))
    {
      return false;
    }

    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var newTask = await GetNextAutoTaskSqlAsync(robotId, realmId);
    if (newTask != null)
    {
      await AddAutoTaskJourney(newTask);
      // Send the running task to the robot
      var task = await GenerateTaskDetail(newTask);
      if (task != null)
      {
        await _autoTaskRepository.AddAutoTaskAsync(realmId, robotId, task);
      }
      // Has new task
      return true;
    }
    return false;
  }

  public async Task ReleaseRobotAsync(Guid robotId)
  {
    var realmId = _robotService.GetRobotRealmIdAsync(robotId).Result ?? 0;
    await _autoTaskRepository.SchedulerReleaseRobotAsync(realmId, robotId);
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
      if (task == null)
      {
        return null;
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

  public async Task AutoTaskAbortAsync(Guid robotId, int taskId, string token, AutoTaskAbortReason autoTaskAbortReason)
  {
    var task = await AutoTaskAbortSqlAsync(taskId, robotId, token);
    if (task == null)
    {
      return;
    }

    await DeleteTriggerRetries(taskId);
    await _emailService.SendAutoTaskAbortEmailAsync(taskId, autoTaskAbortReason);
    await AddAutoTaskJourney(task);
    // Allow update the task to rabbitmq
    var sendTask = await GenerateTaskDetail(task);
    if (!await RunSchedulerRobotReadyAsync(robotId))
    {
      // No new task, send the aborted task to idle the robot
      if (sendTask != null)
      {
        var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
        await _autoTaskRepository.AddAutoTaskAsync(realmId, robotId, sendTask);
      }
    }
  }

  public async Task<bool> AutoTaskAbortApiAsync(int taskId)
  {
    var task = await AutoTaskAbortSqlAsync(taskId);
    if (task == null)
    {
      return false;
    }

    await DeleteTriggerRetries(taskId);
    await _emailService.SendAutoTaskAbortEmailAsync(taskId, AutoTaskAbortReason.UserApi);
    await AddAutoTaskJourney(task);
    // Allow update the task to rabbitmq
    var sendTask = await GenerateTaskDetail(task);
    if (task.AssignedRobotId != null && !await RunSchedulerRobotReadyAsync(task.AssignedRobotId.Value))
    {
      // No new task, send the aborted task to idle the robot
      if (sendTask != null)
      {
        var realmId = await _robotService.GetRobotRealmIdAsync(task.AssignedRobotId!.Value) ?? 0;
        await _autoTaskRepository.AddAutoTaskAsync(realmId, task.AssignedRobotId!.Value, sendTask);
      }
    }
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
      if (task == null)
      {
        return null;
      }

      // Get flow detail
      var flowDetail = await _context.FlowDetails.AsNoTracking()
        .Where(f => f.FlowId == task!.FlowId)
        .Where(f => f.Order > task!.CurrentProgressOrder)
        .Select(f => new
        {
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

  public async Task AutoTaskNextAsync(Guid robotId, int taskId, string token)
  {
    var task = await AutoTaskNextSqlAsync(robotId, taskId, token);
    if (task == null)
    {
      return;
    }

    await AddAutoTaskJourney(task);
    if (task.CurrentProgressId == (int)ProgressState.Completed && await RunSchedulerRobotReadyAsync(robotId))
    {
      // Allow update the task to rabbitmq
      await GenerateTaskDetail(task);
      // Completed task, and new task available
      return;
    }

    // Send the running task / complete task to the robot
    var sendTask = await GenerateTaskDetail(task);
    if (sendTask != null)
    {
      var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
      await _autoTaskRepository.AddAutoTaskAsync(realmId, robotId, sendTask);
    }
  }

  public async Task<bool> AutoTaskNextApiAsync(Guid robotId, int taskId, string token)
  {
    var task = await AutoTaskNextSqlAsync(robotId, taskId, token);
    if (task == null)
    {
      return false;
    }

    await AddAutoTaskJourney(task);
    if (task.CurrentProgressId == (int)ProgressState.Completed && await RunSchedulerRobotReadyAsync(robotId))
    {
      // Allow update the task to rabbitmq
      await GenerateTaskDetail(task);
      // Completed task, and new task available
      return true;
    }

    // Send the running task / complete task to the robot
    var sendTask = await GenerateTaskDetail(task);
    if (sendTask != null)
    {
      var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
      await _autoTaskRepository.AddAutoTaskAsync(realmId, robotId, sendTask);
    }
    return true;
  }
}