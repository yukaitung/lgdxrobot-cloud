using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface IWaypointService
{
  Task<(IEnumerable<WaypointListBusinessModel>, PaginationHelper)> GetWaypointsAsync(int? realmId, string? name, int pageNumber, int pageSize);
  Task<WaypointBusinessModel> GetWaypointAsync(int waypointId);
  Task<WaypointBusinessModel> CreateWaypointAsync(WaypointCreateBusinessModel waypointCreateBusinessModel);
  Task<bool> UpdateWaypointAsync(int waypointId, WaypointUpdateBusinessModel waypointUpdateBusinessModel);
  Task<bool> TestDeleteWaypointAsync(int waypointId);
  Task<bool> DeleteWaypointAsync(int waypointId);
  
  Task<IEnumerable<WaypointSearchBusinessModel>> SearchWaypointsAsync(int realmId, string? name);
}

public class WaypointService(
    IActivityLogService activityLogService,
    LgdxContext context
  ) : IWaypointService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<WaypointListBusinessModel>, PaginationHelper)> GetWaypointsAsync(int? realmId, string? name, int pageNumber, int pageSize)
  {
    var query = _context.Waypoints as IQueryable<Waypoint>;
    if(!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(t => t.Name.ToLower().Contains(name.ToLower()));
    }
    if(realmId != null)
    {
      query = query.Where(t => t.RealmId == realmId);
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var waypoints = await query.AsNoTracking()
      .OrderBy(a => a.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(w => new WaypointListBusinessModel {
        Id = w.Id,
        Name = w.Name,
        RealmId = w.RealmId,
        RealmName = w.Realm.Name,
        X = w.X,
        Y = w.Y,
        Rotation = w.Rotation,
      })
      .AsSplitQuery()
      .ToListAsync();
    return (waypoints, PaginationHelper);
  }

  public async Task<WaypointBusinessModel> GetWaypointAsync(int waypointId)
  {
    return await _context.Waypoints.AsNoTracking()
      .Where(w => w.Id == waypointId)
      .Include(w => w.Realm)
      .Select(w => new WaypointBusinessModel {
        Id = w.Id,
        Name = w.Name,
        RealmId = w.RealmId,
        RealmName = w.Realm.Name,
        X = w.X,
        Y = w.Y,
        Rotation = w.Rotation,
        IsParking = w.IsParking,
        HasCharger = w.HasCharger,
        IsReserved = w.IsReserved,
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  public async Task<WaypointBusinessModel> CreateWaypointAsync(WaypointCreateBusinessModel waypointCreateBusinessModel)
  {
    var realm = await _context.Realms.AsNoTracking()
      .Where(r => r.Id == waypointCreateBusinessModel.RealmId)
      .FirstOrDefaultAsync() 
        ?? throw new LgdxValidation400Expection(nameof(waypointCreateBusinessModel.RealmId), "Realm does not exist.");
    
    var waypoint = new Waypoint {
      Name = waypointCreateBusinessModel.Name,
      RealmId = waypointCreateBusinessModel.RealmId,
      X = waypointCreateBusinessModel.X,
      Y = waypointCreateBusinessModel.Y,
      Rotation = waypointCreateBusinessModel.Rotation,
      IsParking = waypointCreateBusinessModel.IsParking,
      HasCharger = waypointCreateBusinessModel.HasCharger,
      IsReserved = waypointCreateBusinessModel.IsReserved,
    };

    await _context.Waypoints.AddAsync(waypoint);
    await _context.SaveChangesAsync();
    
    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(Waypoint),
      EntityId = waypoint.Id.ToString(),
      Action = ActivityAction.Create,
    });

    return new WaypointBusinessModel
    {
      Id = waypoint.Id,
      Name = waypoint.Name,
      RealmId = waypoint.RealmId,
      RealmName = realm.Name,
      X = waypoint.X,
      Y = waypoint.Y,
      Rotation = waypoint.Rotation,
      IsParking = waypoint.IsParking,
      HasCharger = waypoint.HasCharger,
      IsReserved = waypoint.IsReserved,
    };
  }

  public async Task<bool> UpdateWaypointAsync(int waypointId, WaypointUpdateBusinessModel waypointUpdateBusinessModel)
  {
    bool result = await _context.Waypoints
      .Where(w => w.Id == waypointId)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(w => w.Name, waypointUpdateBusinessModel.Name)
        .SetProperty(w => w.X, waypointUpdateBusinessModel.X)
        .SetProperty(w => w.Y, waypointUpdateBusinessModel.Y)
        .SetProperty(w => w.Rotation, waypointUpdateBusinessModel.Rotation)
        .SetProperty(w => w.IsParking, waypointUpdateBusinessModel.IsParking)
        .SetProperty(w => w.HasCharger, waypointUpdateBusinessModel.HasCharger)
        .SetProperty(w => w.IsReserved, waypointUpdateBusinessModel.IsReserved)
      ) == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Waypoint),
        EntityId = waypointId.ToString(),
        Action = ActivityAction.Update,
      });
    }
    return result;
  }

  public async Task<bool> TestDeleteWaypointAsync(int waypointId)
  {
    var dependencies = await _context.AutoTasksDetail
      .Include(t => t.AutoTask)
      .Where(t => t.WaypointId == waypointId)
      .Where(t => t.AutoTask.CurrentProgressId != (int)ProgressState.Completed && t.AutoTask.CurrentProgressId != (int)ProgressState.Aborted)
      .CountAsync();
    if (dependencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(waypointId), $"This waypoint has been used by {dependencies} running/waiting/template tasks.");
    }
    dependencies = await _context.WaypointTraffics
      .Where(t => t.WaypointFromId == waypointId || t.WaypointToId == waypointId)
      .CountAsync();
    if (dependencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(waypointId), $"This waypoint has {dependencies} traffics.");
    }
    return true;
  }

  public async Task<bool> DeleteWaypointAsync(int waypointId)
  {
    bool result = await _context.Waypoints.Where(w => w.Id == waypointId)
      .ExecuteDeleteAsync() == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Waypoint),
        EntityId = waypointId.ToString(),
        Action = ActivityAction.Delete,
      });
    }
    return result;
  }

  public async Task<IEnumerable<WaypointSearchBusinessModel>> SearchWaypointsAsync(int realmId, string? name)
  {
    var n = name ?? string.Empty;
    return await _context.Waypoints.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .Where(t => t.Name.ToLower().Contains(n.ToLower()))
      .Take(10)
      .Select(m => new WaypointSearchBusinessModel {
        Id = m.Id,
        Name = m.Name,
      })
      .ToListAsync();
  }
}