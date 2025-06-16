using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Automation;

public interface IAutoTaskService
{
  Task<(IEnumerable<AutoTaskListBusinessModel>, PaginationHelper)> GetAutoTasksAsync(int? realmId, string? name, AutoTaskCatrgory? autoTaskCatrgory, int pageNumber = 1, int pageSize = 10);
  Task<AutoTaskBusinessModel> GetAutoTaskAsync(int autoTaskId);
  Task<AutoTaskBusinessModel> CreateAutoTaskAsync(AutoTaskCreateBusinessModel autoTaskCreateBusinessModel);
  Task<bool> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateBusinessModel autoTaskUpdateBusinessModel);
  Task<bool> DeleteAutoTaskAsync(int autoTaskId);

  Task AbortAutoTaskAsync(int autoTaskId);
  Task AutoTaskNextApiAsync(Guid robotId, int taskId, string token);

  Task<AutoTaskStatisticsBusinessModel> GetAutoTaskStatisticsAsync(int realmId);
  Task<AutoTaskListBusinessModel?> GetRobotCurrentTaskAsync(Guid robotId);
}

public class AutoTaskService(
    LgdxContext context,
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IBus bus,
    IEventService eventService,
    IMemoryCache memoryCache,
    IOnlineRobotsService onlineRobotsService
  ) : IAutoTaskService
{
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));

  public async Task<(IEnumerable<AutoTaskListBusinessModel>, PaginationHelper)> GetAutoTasksAsync(int? realmId, string? name, AutoTaskCatrgory? autoTaskCatrgory, int pageNumber = 1, int pageSize = 10)
  {
    var query = _context.AutoTasks as IQueryable<AutoTask>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(t => t.Name != null && t.Name.ToLower().Contains(name.ToLower()));
    }
    if (realmId != null)
    {
      query = query.Where(t => t.RealmId == realmId);
    }
    switch (autoTaskCatrgory)
    {
      case AutoTaskCatrgory.Template:
        query = query.Where(t => t.CurrentProgressId == (int)ProgressState.Template);
        break;
      case AutoTaskCatrgory.Waiting:
        query = query.Where(t => t.CurrentProgressId == (int)ProgressState.Waiting);
        break;
      case AutoTaskCatrgory.Completed:
        query = query.Where(t => t.CurrentProgressId == (int)ProgressState.Completed);
        break;
      case AutoTaskCatrgory.Aborted:
        query = query.Where(t => t.CurrentProgressId == (int)ProgressState.Aborted);
        break;
      case AutoTaskCatrgory.Running:
        query = query.Where(t => t.CurrentProgressId > (int)ProgressState.Aborted);
        break;
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var autoTasks = await query.AsNoTracking()
      .OrderByDescending(t => t.Priority)
      .ThenBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(t => new AutoTaskListBusinessModel
      {
        Id = t.Id,
        Name = t.Name,
        Priority = t.Priority,
        FlowId = t.Flow!.Id,
        FlowName = t.Flow.Name,
        RealmId = t.Realm.Id,
        RealmName = t.Realm.Name,
        AssignedRobotId = t.AssignedRobotId,
        AssignedRobotName = t.AssignedRobot!.Name,
        CurrentProgressId = t.CurrentProgressId,
        CurrentProgressName = t.CurrentProgress.Name,
      })
      .AsSplitQuery()
      .ToListAsync();
    return (autoTasks, PaginationHelper);
  }

  public async Task<AutoTaskBusinessModel> GetAutoTaskAsync(int autoTaskId)
  {
    return await _context.AutoTasks.AsNoTracking()
      .Where(t => t.Id == autoTaskId)
      .Include(t => t.AutoTaskDetails
        .OrderBy(td => td.Order))
        .ThenInclude(td => td.Waypoint)
      .Include(t => t.AutoTaskJourneys)
        .ThenInclude(tj => tj.CurrentProgress)
      .Include(t => t.AssignedRobot)
      .Include(t => t.CurrentProgress)
      .Include(t => t.Flow)
      .Include(t => t.Realm)
      .AsSplitQuery()
      .Select(t => new AutoTaskBusinessModel
      {
        Id = t.Id,
        Name = t.Name,
        Priority = t.Priority,
        FlowId = t.Flow!.Id,
        FlowName = t.Flow.Name,
        RealmId = t.Realm.Id,
        RealmName = t.Realm.Name,
        AssignedRobotId = t.AssignedRobotId,
        AssignedRobotName = t.AssignedRobot!.Name,
        CurrentProgressId = t.CurrentProgressId,
        CurrentProgressName = t.CurrentProgress.Name,
        AutoTaskJourneys = t.AutoTaskJourneys.Select(tj => new AutoTaskJourneyBusinessModel
        {
          Id = tj.Id,
          CurrentProcessId = tj.CurrentProgressId,
          CurrentProcessName = tj.CurrentProgress == null ? null : tj.CurrentProgress.Name,
          CreatedAt = tj.CreatedAt,
        })
        .OrderBy(tj => tj.Id)
        .ToList(),
        AutoTaskDetails = t.AutoTaskDetails.Select(td => new AutoTaskDetailBusinessModel
        {
          Id = td.Id,
          Order = td.Order,
          CustomX = td.CustomX,
          CustomY = td.CustomY,
          CustomRotation = td.CustomRotation,
          Waypoint = td.Waypoint == null ? null : new WaypointBusinessModel
          {
            Id = td.Waypoint.Id,
            Name = td.Waypoint.Name,
            RealmId = t.Realm.Id,
            RealmName = t.Realm.Name,
            X = td.Waypoint.X,
            Y = td.Waypoint.Y,
            Rotation = td.Waypoint.Rotation,
            IsParking = td.Waypoint.IsParking,
            HasCharger = td.Waypoint.HasCharger,
            IsReserved = td.Waypoint.IsReserved,
          },
        })
        .OrderBy(td => td.Order)
        .ToList()
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  private async Task ValidateAutoTask(HashSet<int> waypointIds, int flowId, int realmId, Guid? robotId)
  {
    // Validate the Waypoint IDs
    var waypointDict = await _context.Waypoints.AsNoTracking()
      .Where(w => waypointIds.Contains(w.Id))
      .Where(w => w.RealmId == realmId)
      .ToDictionaryAsync(w => w.Id, w => w);
    foreach (var waypointId in waypointIds)
    {
      if (!waypointDict.ContainsKey(waypointId))
      {
        throw new LgdxValidation400Expection(nameof(Waypoint), $"The Waypoint ID {waypointId} is invalid.");
      }
    }
    // Validate the Flow ID
    var flow = await _context.Flows.Where(f => f.Id == flowId).AnyAsync();
    if (flow == false)
    {
      throw new LgdxValidation400Expection(nameof(Flow), $"The Flow ID {flowId} is invalid.");
    }
    // Validate the Realm ID
    var realm = await _context.Realms.Where(r => r.Id == realmId).AnyAsync();
    if (realm == false)
    {
      throw new LgdxValidation400Expection(nameof(Realm), $"The Realm ID {realmId} is invalid.");
    }
    // Validate the Assigned Robot ID
    if (robotId != null)
    {
      var robot = await _context
        .Robots
        .Where(r => r.Id == robotId)
        .Where(r => r.RealmId == realmId)
        .AnyAsync();
      if (robot == false)
      {
        throw new LgdxValidation400Expection(nameof(Robot), $"Robot ID: {robotId} is invalid.");
      }
    }
  }

  public async Task<AutoTaskBusinessModel> CreateAutoTaskAsync(AutoTaskCreateBusinessModel autoTaskCreateBusinessModel)
  {
    // Waypoint is needed when Waypoins Traffic Control is enabled
    bool hasWaypointsTrafficControl = await _context.Realms.AsNoTracking()
      .Where(r => r.Id == autoTaskCreateBusinessModel.RealmId)
      .Select(r => r.HasWaypointsTrafficControl)
      .FirstOrDefaultAsync();
    if (hasWaypointsTrafficControl)
    {
      foreach (var detail in autoTaskCreateBusinessModel.AutoTaskDetails)
      {
        if (detail.WaypointId == null)
        {
          throw new LgdxValidation400Expection(nameof(detail.WaypointId), "Waypoint is required when Waypoints Traffic Control is enabled.");
        }
      }
    }

    HashSet<int> waypointIds = autoTaskCreateBusinessModel.AutoTaskDetails
      .Where(d => d.WaypointId != null)
      .Select(d => d.WaypointId!.Value)
      .ToHashSet();

    await ValidateAutoTask(waypointIds,
      autoTaskCreateBusinessModel.FlowId,
      autoTaskCreateBusinessModel.RealmId,
      autoTaskCreateBusinessModel.AssignedRobotId);

    var autoTask = new AutoTask
    {
      Name = autoTaskCreateBusinessModel.Name,
      Priority = autoTaskCreateBusinessModel.Priority,
      FlowId = autoTaskCreateBusinessModel.FlowId,
      RealmId = autoTaskCreateBusinessModel.RealmId,
      AssignedRobotId = autoTaskCreateBusinessModel.AssignedRobotId,
      CurrentProgressId = autoTaskCreateBusinessModel.IsTemplate
        ? (int)ProgressState.Template
        : (int)ProgressState.Waiting,
      AutoTaskDetails = autoTaskCreateBusinessModel.AutoTaskDetails.Select(td => new AutoTaskDetail
      {
        Order = td.Order,
        CustomX = td.CustomX,
        CustomY = td.CustomY,
        CustomRotation = td.CustomRotation,
        WaypointId = td.WaypointId,
      })
      .OrderBy(td => td.Order)
      .ToList(),
    };
    await _context.AutoTasks.AddAsync(autoTask);
    await _context.SaveChangesAsync();
    _autoTaskSchedulerService.ResetIgnoreRobot(autoTask.RealmId);
    eventService.AutoTaskHasCreated();

    var autoTaskBusinessModel = await _context.AutoTasks.AsNoTracking()
      .Where(t => t.Id == autoTask.Id)
      .Select(t => new AutoTaskBusinessModel
      {
        Id = t.Id,
        Name = t.Name,
        Priority = t.Priority,
        FlowId = t.Flow!.Id,
        FlowName = t.Flow.Name,
        RealmId = t.Realm.Id,
        RealmName = t.Realm.Name,
        AssignedRobotId = t.AssignedRobotId,
        AssignedRobotName = t.AssignedRobot!.Name,
        CurrentProgressId = t.CurrentProgressId,
        CurrentProgressName = t.CurrentProgress.Name,
        AutoTaskDetails = t.AutoTaskDetails.Select(td => new AutoTaskDetailBusinessModel
        {
          Id = td.Id,
          Order = td.Order,
          CustomX = td.CustomX,
          CustomY = td.CustomY,
          CustomRotation = td.CustomRotation,
          Waypoint = td.Waypoint == null ? null : new WaypointBusinessModel
          {
            Id = td.Waypoint.Id,
            Name = td.Waypoint.Name,
            RealmId = t.Realm.Id,
            RealmName = t.Realm.Name,
            X = td.Waypoint.X,
            Y = td.Waypoint.Y,
            Rotation = td.Waypoint.Rotation,
            IsParking = td.Waypoint.IsParking,
            HasCharger = td.Waypoint.HasCharger,
            IsReserved = td.Waypoint.IsReserved,
          },
        })
        .OrderBy(td => td.Order)
        .ToList()
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();

    if (autoTask.CurrentProgressId == (int)ProgressState.Waiting)
    {
      var autoTaskJourney = new AutoTaskJourney
      {
        AutoTaskId = autoTaskBusinessModel.Id,
        CurrentProgressId = autoTask.CurrentProgressId
      };
      await _context.AutoTasksJourney.AddAsync(autoTaskJourney);
      await _context.SaveChangesAsync();

      await _bus.Publish(autoTaskBusinessModel.ToContract());
    }

    return autoTaskBusinessModel;
  }

  public async Task<bool> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateBusinessModel autoTaskUpdateBusinessModel)
  {
    var task = await _context.AutoTasks
      .Where(t => t.Id == autoTaskId)
      .Include(t => t.AutoTaskDetails
        .OrderBy(td => td.Order))
      .FirstOrDefaultAsync()
      ?? throw new LgdxNotFound404Exception();

    if (task.CurrentProgressId != (int)ProgressState.Template)
    {
      throw new LgdxValidation400Expection("AutoTaskId", "Only AutoTask Templates are editable.");
    }
    // Ensure the input task does not include Detail ID from other task
    HashSet<int> dbDetailIds = task.AutoTaskDetails.Select(d => d.Id).ToHashSet();
    foreach (var bmDetailId in autoTaskUpdateBusinessModel.AutoTaskDetails.Where(d => d.Id != null).Select(d => d.Id))
    {
      if (bmDetailId != null && !dbDetailIds.Contains((int)bmDetailId))
      {
        throw new LgdxValidation400Expection("AutoTaskDetailId", $"The Task Detail ID {(int)bmDetailId} is belongs to other Task.");
      }
    }
    // Waypoint is needed when Waypoins Traffic Control is enabled
    bool hasWaypointsTrafficControl = await _context.Realms.AsNoTracking()
      .Where(r => r.Id == task.RealmId)
      .Select(r => r.HasWaypointsTrafficControl)
      .FirstOrDefaultAsync();
    if (hasWaypointsTrafficControl)
    {
      foreach (var detail in autoTaskUpdateBusinessModel.AutoTaskDetails)
      {
        if (detail.WaypointId == null)
        {
          throw new LgdxValidation400Expection(nameof(detail.WaypointId), "Waypoint is required when Waypoints Traffic Control is enabled.");
        }
      }
    }
    HashSet<int> waypointIds = autoTaskUpdateBusinessModel.AutoTaskDetails
      .Where(d => d.WaypointId != null)
      .Select(d => d.WaypointId!.Value)
      .ToHashSet();
    await ValidateAutoTask(waypointIds,
      autoTaskUpdateBusinessModel.FlowId,
      task.RealmId,
      autoTaskUpdateBusinessModel.AssignedRobotId);

    task.Name = autoTaskUpdateBusinessModel.Name;
    task.Priority = autoTaskUpdateBusinessModel.Priority;
    task.FlowId = autoTaskUpdateBusinessModel.FlowId;
    task.AssignedRobotId = autoTaskUpdateBusinessModel.AssignedRobotId;
    task.AutoTaskDetails = autoTaskUpdateBusinessModel.AutoTaskDetails.Select(td => new AutoTaskDetail
    {
      Id = (int)td.Id!,
      Order = td.Order,
      CustomX = td.CustomX,
      CustomY = td.CustomY,
      CustomRotation = td.CustomRotation,
      WaypointId = td.WaypointId,
    }).ToList();
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<bool> DeleteAutoTaskAsync(int autoTaskId)
  {
    var autoTask = await _context.AutoTasks.AsNoTracking()
      .Where(t => t.Id == autoTaskId)
      .FirstOrDefaultAsync()
      ?? throw new LgdxNotFound404Exception();
    if (autoTask.CurrentProgressId != (int)ProgressState.Template)
    {
      throw new LgdxValidation400Expection("AutoTaskId", "Cannot delete the task not in running status.");
    }

    _context.AutoTasks.Remove(autoTask);
    await _context.SaveChangesAsync();

    return true;
  }

  public async Task AbortAutoTaskAsync(int autoTaskId)
  {
    var autoTask = await _context.AutoTasks.AsNoTracking()
      .Where(t => t.Id == autoTaskId)
      .FirstOrDefaultAsync()
      ?? throw new LgdxNotFound404Exception();

    if (autoTask.CurrentProgressId == (int)ProgressState.Template ||
        autoTask.CurrentProgressId == (int)ProgressState.Completed ||
        autoTask.CurrentProgressId == (int)ProgressState.Aborted)
    {
      throw new LgdxValidation400Expection(nameof(autoTaskId), "Cannot abort the task not in running status.");
    }
    if (autoTask.CurrentProgressId != (int)ProgressState.Waiting &&
        autoTask.AssignedRobotId != null &&
        await _onlineRobotsService.SetAbortTaskAsync((Guid)autoTask.AssignedRobotId!, true))
    {
      // If the robot is online, abort the task from the robot
      return;
    }
    else
    {
      await _autoTaskSchedulerService.AutoTaskAbortApiAsync(autoTask.Id);
    }
  }

  public async Task AutoTaskNextApiAsync(Guid robotId, int taskId, string token)
  {
    var result = await _autoTaskSchedulerService.AutoTaskNextApiAsync(robotId, taskId, token)
      ?? throw new LgdxValidation400Expection(nameof(token), "The next token is invalid.");
    _onlineRobotsService.SetAutoTaskNextApi(robotId, result);
  }

  public async Task<AutoTaskStatisticsBusinessModel> GetAutoTaskStatisticsAsync(int realmId)
  {
    _memoryCache.TryGetValue("Automation_Statistics", out AutoTaskStatisticsBusinessModel? autoTaskStatistics);
    if (autoTaskStatistics != null)
    {
      return autoTaskStatistics;
    }

    /*
     * Get queuing tasks and running tasks (total)
     */
    DateTime CurrentDate = DateTime.UtcNow;
    var waitingTaskCount = await _context.AutoTasks.AsNoTracking()
      .Where(t => t.CurrentProgressId == (int)ProgressState.Waiting)
      .CountAsync();
    var waitingTaskPerHour = await _context.AutoTasksJourney.AsNoTracking()
    .Where(t => t.CurrentProgressId == (int)ProgressState.Waiting && t.CreatedAt > DateTime.UtcNow.AddHours(-23))
    .GroupBy(t => new
    {
      t.CreatedAt.Year,
      t.CreatedAt.Month,
      t.CreatedAt.Day,
      t.CreatedAt.Hour
    })
    .Select(g => new
    {
      Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
      TasksCompleted = g.Count()
    })
    .OrderBy(g => g.Time)
    .ToListAsync();
    Dictionary<int, int> waitingTaskPerHourDict = [];
    foreach (var taskCount in waitingTaskPerHour)
    {
      waitingTaskPerHourDict.Add((int)(CurrentDate - taskCount.Time).TotalHours, taskCount.TasksCompleted);
    }

    var runningTaskCount = await _context.AutoTasks.AsNoTracking()
      .Where(t => !LgdxHelper.AutoTaskStaticStates.Contains(t.CurrentProgressId!))
      .CountAsync();
    var runningTaskPerHour = await _context.AutoTasksJourney.AsNoTracking()
    .Where(t => !LgdxHelper.AutoTaskStaticStates.Contains((int)t.CurrentProgressId!) && t.CreatedAt > DateTime.UtcNow.AddHours(-23))
    .GroupBy(t => new
    {
      t.CreatedAt.Year,
      t.CreatedAt.Month,
      t.CreatedAt.Day,
      t.CreatedAt.Hour
    })
    .Select(g => new
    {
      Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
      TasksCompleted = g.Count()
    })
    .OrderBy(g => g.Time)
    .ToListAsync();
    Dictionary<int, int> runningTaskPerHourDict = [];
    foreach (var taskCount in runningTaskPerHour)
    {
      runningTaskPerHourDict.Add((int)(CurrentDate - taskCount.Time).TotalHours, taskCount.TasksCompleted);
    }

    /*
     * Get task completed / aborted in the last 24 hours
     */
    var completedTaskPerHour = await _context.AutoTasksJourney.AsNoTracking()
    .Where(t => t.CurrentProgressId == (int)ProgressState.Completed && t.CreatedAt > DateTime.UtcNow.AddHours(-23))
    .GroupBy(t => new
    {
      t.CreatedAt.Year,
      t.CreatedAt.Month,
      t.CreatedAt.Day,
      t.CreatedAt.Hour
    })
    .Select(g => new
    {
      Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
      TasksCompleted = g.Count()
    })
    .OrderBy(g => g.Time)
    .ToListAsync();
    Dictionary<int, int> completedTaskPerHourDict = [];
    foreach (var taskCount in completedTaskPerHour)
    {
      completedTaskPerHourDict.Add((int)(CurrentDate - taskCount.Time).TotalHours, taskCount.TasksCompleted);
    }

    var abortedTaskPerHour = await _context.AutoTasksJourney.AsNoTracking()
    .Where(t => t.CurrentProgressId == (int)ProgressState.Aborted && t.CreatedAt > DateTime.UtcNow.AddHours(-23))
    .GroupBy(t => new
    {
      t.CreatedAt.Year,
      t.CreatedAt.Month,
      t.CreatedAt.Day,
      t.CreatedAt.Hour
    })
    .Select(g => new
    {
      Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
      TasksCompleted = g.Count()
    })
    .OrderBy(g => g.Time)
    .ToListAsync();
    Dictionary<int, int> abortedTaskPerHourDict = [];
    foreach (var taskCount in abortedTaskPerHour)
    {
      abortedTaskPerHourDict.Add((int)(CurrentDate - taskCount.Time).TotalHours, taskCount.TasksCompleted);
    }

    AutoTaskStatisticsBusinessModel statistics = new()
    {
      LastUpdatedAt = DateTime.Now,
      WaitingTasks = waitingTaskCount,
      RunningTasks = runningTaskCount,
    };

    /*
     * Get trends
     * Note that the completed tasks and aborted tasks are looked up in the last 24 hours
     */
    // Now = total tasks
    statistics.WaitingTasksTrend.Add(waitingTaskCount);
    statistics.RunningTasksTrend.Add(runningTaskCount);
    for (int i = 0; i < 23; i++)
    {
      statistics.WaitingTasksTrend.Add(waitingTaskPerHourDict.TryGetValue(i, out int count) ? count : 0);
      statistics.RunningTasksTrend.Add(runningTaskPerHourDict.TryGetValue(i, out count) ? count : 0);
      statistics.CompletedTasksTrend.Add(completedTaskPerHourDict.TryGetValue(i, out count) ? count : 0);
      statistics.AbortedTasksTrend.Add(abortedTaskPerHourDict.TryGetValue(i, out count) ? count : 0);
    }
    statistics.WaitingTasksTrend.Reverse();
    statistics.RunningTasksTrend.Reverse();
    statistics.CompletedTasksTrend.Reverse();
    statistics.CompletedTasks = statistics.CompletedTasksTrend.Sum();
    statistics.AbortedTasksTrend.Reverse();
    statistics.AbortedTasks = statistics.AbortedTasksTrend.Sum();
    _memoryCache.Set("Automation_Statistics", statistics, TimeSpan.FromMinutes(5));

    return statistics;
  }

  public async Task<AutoTaskListBusinessModel?> GetRobotCurrentTaskAsync(Guid robotId)
  {
    var autoTask = await _context.AutoTasks.AsNoTracking()
      .Where(t => !LgdxHelper.AutoTaskStaticStates.Contains(t.CurrentProgressId!) && t.AssignedRobotId == robotId)
      .Select(t => new AutoTaskListBusinessModel
      {
        Id = t.Id,
        Name = t.Name,
        Priority = t.Priority,
        FlowId = t.Flow!.Id,
        FlowName = t.Flow.Name,
        RealmId = t.Realm.Id,
        RealmName = t.Realm.Name,
        AssignedRobotId = t.AssignedRobotId,
        AssignedRobotName = t.AssignedRobot!.Name,
        CurrentProgressId = t.CurrentProgressId,
        CurrentProgressName = t.CurrentProgress.Name,
      })
      .FirstOrDefaultAsync();
    return autoTask;
  }
}