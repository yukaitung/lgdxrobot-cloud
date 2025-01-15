using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Automation;
using LGDXRobot2Cloud.Data.Models.Business.Navigation;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Automation;

public interface IAutoTaskService
{
  Task<(IEnumerable<AutoTaskListBusinessModel>, PaginationHelper)> GetAutoTasksAsync(int? realm, string? name, int? showProgressId, bool? showRunningTasks, int pageNumber = 1, int pageSize = 10);
  Task<AutoTaskBusinessModel> GetAutoTaskAsync(int autoTaskId);
  Task<AutoTaskBusinessModel> CreateAutoTaskAsync(AutoTaskCreateBusinessModel autoTaskCreateBusinessModel);
  Task<bool> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateBusinessModel autoTaskUpdateBusinessModel);
  Task<bool> DeleteAutoTaskAsync(int autoTaskId);

  Task AbortAutoTaskAsync(int autoTaskId);
  Task AutoTaskNextApiAsync(Guid robotId, int taskId, string token);

  Task<AutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token);
  Task<AutoTask?> AutoTaskAbortManualAsync(int taskId);
}

public class AutoTaskService(
    LgdxContext context,
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IOnlineRobotsService onlineRobotsService
  ) : IAutoTaskService
{
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));

  public async Task<(IEnumerable<AutoTaskListBusinessModel>, PaginationHelper)> GetAutoTasksAsync(int? realm, string? name, int? showProgressId, bool? showRunningTasks, int pageNumber = 1, int pageSize = 10)
  {
    var query = _context.AutoTasks as IQueryable<AutoTask>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(t => t.Name != null && t.Name.Contains(name));
    }
      if (showProgressId != null)
      query = query.Where(t => t.CurrentProgressId == showProgressId);
    if (showRunningTasks == true)
      query = query.Where(t => t.CurrentProgressId > (int)ProgressState.Aborted);
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var autoTasks = await query.AsNoTracking()
      .OrderByDescending(t => t.Priority)
      .ThenBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Include(t => t.AssignedRobot)
      .Include(t => t.CurrentProgress)
      .Include(t => t.Flow)
      .Include(t => t.Realm)
      .Select(t => new AutoTaskListBusinessModel {
        Id = t.Id,
        Name = t.Name,
        Priority = t.Priority,
        FlowId = t.Flow.Id,
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
      .Include(t => t.AssignedRobot)
      .Include(t => t.CurrentProgress)
      .Include(t => t.Flow)
      .Include(t => t.Realm)
      .Select(t => new AutoTaskBusinessModel {
        Id = t.Id,
        Name = t.Name,
        Priority = t.Priority,
        FlowId = t.Flow.Id,
        FlowName = t.Flow.Name,
        RealmId = t.Realm.Id, 
        RealmName = t.Realm.Name,
        AssignedRobotId = t.AssignedRobotId,
        AssignedRobotName = t.AssignedRobot!.Name,
        CurrentProgressId = t.CurrentProgressId,
        CurrentProgressName = t.CurrentProgress.Name,
        AutoTaskDetails = t.AutoTaskDetails.Select(td => new AutoTaskDetailBusinessModel {
          Id = td.Id,
          Order = td.Order,
          CustomX = td.CustomX,
          CustomY = td.CustomY,
          CustomRotation = td.CustomRotation,
          Waypoint = td.Waypoint == null ? null : new WaypointBusinessModel {
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
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  private async Task ValidateAutoTask(HashSet<int> waypointIds, int flowId, int realmId, Guid? robotId)
  {
    // Validate the Waypoint IDs
    var waypointDict = await _context.Waypoints.AsNoTracking().Where(w => waypointIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w);
    foreach(var waypointId in waypointIds)
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
      var robot = await _context.Robots.Where(r => r.Id == robotId).AnyAsync();
      if (robot == false)
      {
        throw new LgdxValidation400Expection(nameof(Robot), $"Robot ID: {robotId} is invalid.");
      }
    }
  }

  public async Task<AutoTaskBusinessModel> CreateAutoTaskAsync(AutoTaskCreateBusinessModel autoTaskCreateBusinessModel)
  {
    HashSet<int> waypointIds = autoTaskCreateBusinessModel.AutoTaskDetails
      .Where(d => d.WaypointId != null)
      .Select(d => d.WaypointId!.Value)
      .ToHashSet();
    
    await ValidateAutoTask(waypointIds, 
      autoTaskCreateBusinessModel.FlowId, 
      autoTaskCreateBusinessModel.RealmId, 
      autoTaskCreateBusinessModel.AssignedRobotId);

    var autoTask = new AutoTask {
      Name = autoTaskCreateBusinessModel.Name,
      Priority = autoTaskCreateBusinessModel.Priority,
      FlowId = autoTaskCreateBusinessModel.FlowId,
      RealmId = autoTaskCreateBusinessModel.RealmId,
      AssignedRobotId = autoTaskCreateBusinessModel.AssignedRobotId,
      CurrentProgressId = autoTaskCreateBusinessModel.IsTemplate 
        ? (int)ProgressState.Template
        : (int)ProgressState.Waiting,
      AutoTaskDetails = autoTaskCreateBusinessModel.AutoTaskDetails.Select(td => new AutoTaskDetail {
        Order = td.Order,
        CustomX = td.CustomX,
        CustomY = td.CustomY,
        CustomRotation = td.CustomRotation,
        WaypointId = td.WaypointId,
      }).ToList(),
    };
    await _context.AutoTasks.AddAsync(autoTask);
    await _context.SaveChangesAsync();
    await _autoTaskSchedulerService.ResetIgnoreRobotAsync();

    return new AutoTaskBusinessModel {
      Id = autoTask.Id,
      Name = autoTask.Name,
      Priority = autoTask.Priority,
      FlowId = autoTask.FlowId,
      FlowName = autoTask.Flow.Name,
      RealmId = autoTask.RealmId,
      RealmName = autoTask.Realm.Name,
      AssignedRobotId = autoTask.AssignedRobotId,
      AssignedRobotName = autoTask.AssignedRobot?.Name,
      CurrentProgressId = autoTask.CurrentProgressId,
      CurrentProgressName = autoTask.CurrentProgress.Name,
      AutoTaskDetails = autoTask.AutoTaskDetails.Select(td => new AutoTaskDetailBusinessModel {
        Id = td.Id,
        Order = td.Order,
        CustomX = td.CustomX,
        CustomY = td.CustomY,
        CustomRotation = td.CustomRotation,
        Waypoint = td.Waypoint == null ? null : new WaypointBusinessModel {
          Id = td.Waypoint.Id,
          Name = td.Waypoint.Name,
          RealmId = autoTask.RealmId,
          RealmName = autoTask.Realm.Name,
          X = td.Waypoint.X,
          Y = td.Waypoint.Y,
          Rotation = td.Waypoint.Rotation,
          IsParking = td.Waypoint.IsParking,
          HasCharger = td.Waypoint.HasCharger,
          IsReserved = td.Waypoint.IsReserved,
        },
      }).ToList(),
    };
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
    foreach(var bmDetailId in autoTaskUpdateBusinessModel.AutoTaskDetails.Where(d => d.Id != null).Select(d => d.Id))
    {
      if (bmDetailId != null && !dbDetailIds.Contains((int)bmDetailId))
      {
        throw new LgdxValidation400Expection("AutoTaskDetailId", $"The Task Detail ID {(int)bmDetailId} is belongs to other Task.");
      }
    }
    HashSet<int> waypointIds = autoTaskUpdateBusinessModel.AutoTaskDetails
      .Where(d => d.WaypointId != null)
      .Select(d => d.WaypointId!.Value)
      .ToHashSet();
    await ValidateAutoTask(waypointIds, 
      autoTaskUpdateBusinessModel.FlowId, 
      autoTaskUpdateBusinessModel.RealmId, 
      autoTaskUpdateBusinessModel.AssignedRobotId);
    
    task.Name = autoTaskUpdateBusinessModel.Name;
    task.Priority = autoTaskUpdateBusinessModel.Priority;
    task.FlowId = autoTaskUpdateBusinessModel.FlowId;
    task.RealmId = autoTaskUpdateBusinessModel.RealmId;
    task.AssignedRobotId = autoTaskUpdateBusinessModel.AssignedRobotId;
    task.AutoTaskDetails = autoTaskUpdateBusinessModel.AutoTaskDetails.Select(td => new AutoTaskDetail {
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
      await AutoTaskAbortManualAsync(autoTask.Id);
    }
  }

  public async Task AutoTaskNextApiAsync(Guid robotId, int taskId, string token)
  {
    var result = await AutoTaskNextAsync(robotId, taskId, token) 
      ?? throw new LgdxValidation400Expection(nameof(token), "The next token is invalid.");
    _onlineRobotsService.SetAutoTaskNext(robotId, result);
  }


  public async Task<AutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token)
  {
    var result = await _context.AutoTasks.FromSql($"CALL auto_task_next({robotId}, {taskId}, {token});").ToListAsync();
    if (result.Count > 0)
      return result[0];
    else
      return null;
  }

  public async Task<AutoTask?> AutoTaskAbortManualAsync(int taskId)
  {
    var result = await _context.AutoTasks.FromSql($"CALL auto_task_abort_manual({taskId});").ToListAsync();
    if (result.Count > 0)
      return result[0];
    else
      return null;
  }
}