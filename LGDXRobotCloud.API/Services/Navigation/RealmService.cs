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

public interface IRealmService
{
  Task<(IEnumerable<RealmListBusinessModel>, PaginationHelper)> GetRealmsAsync(string? name, int pageNumber, int pageSize);
  Task<RealmBusinessModel> GetRealmAsync(int id);
  Task<RealmBusinessModel> GetDefaultRealmAsync();
  Task<RealmBusinessModel> CreateRealmAsync(RealmCreateBusinessModel createModel);
  Task<bool> UpdateRealmAsync(int id, RealmUpdateBusinessModel updateModel);
  Task<bool> TestDeleteRealmAsync(int id);
  Task<bool> DeleteRealmAsync(int id);

  Task<IEnumerable<RealmSearchBusinessModel>> SearchRealmsAsync(string? name);
}

public class RealmService(
    IActivityLogService activityLogService,
    LgdxContext context
  ) : IRealmService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<RealmListBusinessModel>, PaginationHelper)> GetRealmsAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Realms as IQueryable<Realm>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(m => m.Name.ToLower().Contains(name.ToLower()));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var realms = await query.AsNoTracking()
      .OrderBy(m => m.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(m => new RealmListBusinessModel {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        Resolution = m.Resolution,
      })
      .ToListAsync();
    return (realms, PaginationHelper);
  }

  public async Task<RealmBusinessModel> GetRealmAsync(int id)
  {
    return await _context.Realms.AsNoTracking()
      .Where(m => m.Id == id)
      .Select(m => new RealmBusinessModel {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        HasWaypointsTrafficControl = m.HasWaypointsTrafficControl,
        Image = Convert.ToBase64String(m.Image),
        Resolution = m.Resolution,
        OriginX = m.OriginX,
        OriginY = m.OriginY,
        OriginRotation = m.OriginRotation,
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  public async Task<RealmBusinessModel> GetDefaultRealmAsync()
  {
    return await _context.Realms.AsNoTracking()
      .OrderBy(m => m.Id)
      .Select(m => new RealmBusinessModel {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        HasWaypointsTrafficControl = m.HasWaypointsTrafficControl,
        Image = Convert.ToBase64String(m.Image),
        Resolution = m.Resolution,
        OriginX = m.OriginX,
        OriginY = m.OriginY,
        OriginRotation = m.OriginRotation,
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  public async Task<RealmBusinessModel> CreateRealmAsync(RealmCreateBusinessModel createModel)
  {
    var realm = new Realm {
      Name = createModel.Name,
      Description = createModel.Description,
      HasWaypointsTrafficControl = createModel.HasWaypointsTrafficControl,
      Image = Convert.FromBase64String(createModel.Image),
      Resolution = createModel.Resolution,
      OriginX = createModel.OriginX,
      OriginY = createModel.OriginY,
      OriginRotation = createModel.OriginRotation,
    };

    await _context.Realms.AddAsync(realm);
    await _context.SaveChangesAsync();
    
    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(Realm),
      EntityId = realm.Id.ToString(),
      Action = ActivityAction.Create,
    });

    return new RealmBusinessModel
    {
      Id = realm.Id,
      Name = realm.Name,
      Description = realm.Description,
      HasWaypointsTrafficControl = realm.HasWaypointsTrafficControl,
      Image = createModel.Image,
      Resolution = realm.Resolution,
      OriginX = realm.OriginX,
      OriginY = realm.OriginY,
      OriginRotation = realm.OriginRotation,
    };
  }

  public async Task<bool> UpdateRealmAsync(int id, RealmUpdateBusinessModel updateModel)
  {
    bool result = await _context.Realms
      .Where(m => m.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(m => m.Name, updateModel.Name)
        .SetProperty(m => m.Description, updateModel.Description)
        .SetProperty(m => m.HasWaypointsTrafficControl, updateModel.HasWaypointsTrafficControl)
        .SetProperty(m => m.Image, Convert.FromBase64String(updateModel.Image))
        .SetProperty(m => m.Resolution, updateModel.Resolution)
        .SetProperty(m => m.OriginX, updateModel.OriginX)
        .SetProperty(m => m.OriginY, updateModel.OriginY)
        .SetProperty(m => m.OriginRotation, updateModel.OriginRotation)
      ) == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Realm),
        EntityId = id.ToString(),
        Action = ActivityAction.Update,
      });
    }
    return result;
  }

  public async Task<bool> TestDeleteRealmAsync(int id)
  {
    var dependencies = await _context.Robots.Where(m => m.RealmId == id).CountAsync();
    if (dependencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(id), $"This realm has been used by {dependencies} robots.");
    }
    dependencies = await _context.Waypoints.Where(m => m.RealmId == id).CountAsync();
    if (dependencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(id), $"This realm has been used by {dependencies} waypoints.");
    }
    dependencies = await _context.AutoTasks
      .Where(m => m.RealmId == id)
      .Where(m => m.CurrentProgressId != (int)ProgressState.Completed && m.CurrentProgressId != (int)ProgressState.Aborted)
      .CountAsync();
    if (dependencies > 0)
    {
      throw new LgdxValidation400Expection(nameof(id), $"This realm has been used by {dependencies} running/waiting/template tasks.");
    }
    return true;
  }

  public async Task<bool> DeleteRealmAsync(int id)
  {
    bool result = await _context.Realms.Where(m => m.Id == id)
      .ExecuteDeleteAsync() == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Realm),
        EntityId = id.ToString(),
        Action = ActivityAction.Delete,
      });
    }
    return result;
  }

  public async Task<IEnumerable<RealmSearchBusinessModel>> SearchRealmsAsync(string? name)
  {
    var n = name ?? string.Empty;
    return await _context.Realms.AsNoTracking()
      .Where(w => w.Name.ToLower().Contains(n.ToLower()))
      .Take(10)
      .Select(m => new RealmSearchBusinessModel {
        Id = m.Id,
        Name = m.Name,
      })
      .ToListAsync();
  }
}