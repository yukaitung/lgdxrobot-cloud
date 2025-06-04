using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface IMapEditorService
{
  Task<MapEditorBusinessModel> GetMapAsync(int realmId);
  Task<bool> UpdateMapAsync(int realmId, MapEditorUpdateBusinessModel MapEditUpdateBusinessModel);
}

public class MapEditorService(LgdxContext context) : IMapEditorService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<MapEditorBusinessModel> GetMapAsync(int realmId)
  {
    // Check if realm exists
    var realm = await _context.Realms.AsNoTracking()
      .Where(r => r.Id == realmId)
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();

    var waypoints = await _context.Waypoints.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .Select(w => new WaypointListBusinessModel
      {
        Id = w.Id,
        Name = w.Name,
        RealmId = w.RealmId,
        RealmName = w.Realm.Name,
        X = w.X,
        Y = w.Y,
        Rotation = w.Rotation,
      })
      .ToListAsync();
    var waypointTraffics = await _context.WaypointTraffics.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .Select(w => new WaypointTrafficBusinessModel
      {
        Id = w.Id,
        WaypointFromId = w.WaypointFromId,
        WaypointToId = w.WaypointToId,
      })
      .ToListAsync();
    return new MapEditorBusinessModel
    {
      Waypoints = waypoints,
      WaypointTraffics = waypointTraffics,
    };
  }

  public async Task<bool> UpdateMapAsync(int realmId, MapEditorUpdateBusinessModel mapEditorUpdateBusinessModel)
  {
    // Delete
    foreach (var traffic in mapEditorUpdateBusinessModel.TrafficsToDelete)
    {
      await _context.WaypointTraffics
        .Where(w => w.RealmId == realmId && w.WaypointFromId == traffic.WaypointFromId && w.WaypointToId == traffic.WaypointToId)
        .ExecuteDeleteAsync();
    }
    // Add
    _context.WaypointTraffics.AddRange(mapEditorUpdateBusinessModel.TrafficsToAdd
      .Select(l => new WaypointTraffic
      {
        RealmId = realmId,
        WaypointFromId = l.WaypointFromId,
        WaypointToId = l.WaypointToId,
      }
    ));
    await _context.SaveChangesAsync();

    return true;
  }
}