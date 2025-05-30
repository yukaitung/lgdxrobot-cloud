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
    var waypointLinks = await _context.WaypointLinks.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .Select(w => new WaypointLinkBusinessModel
      {
        Id = w.Id,
        WaypointFromId = w.WaypointFromId,
        WaypointToId = w.WaypointToId,
      })
      .ToListAsync();
    return new MapEditorBusinessModel
    {
      Waypoints = waypoints,
      WaypointLinks = waypointLinks,
    };
  }

  public async Task<bool> UpdateMapAsync(int realmId, MapEditorUpdateBusinessModel mapEditorUpdateBusinessModel)
  {
    var existingLinks = await _context.WaypointLinks
      .Where(w => w.RealmId == realmId)
      .Select(w => new WaypointLinkUpdateBusinessModel
      {
        Id = w.Id,
        WaypointFromId = w.WaypointFromId,
        WaypointToId = w.WaypointToId,
      })
      .ToListAsync();
    var linkToAdd = mapEditorUpdateBusinessModel.WaypointLinks.Except(existingLinks);
    _context.WaypointLinks.AddRange(linkToAdd
      .Select(l => new WaypointLink
      {
        Id = (int)l.Id!,
        RealmId = realmId,
        WaypointFromId = l.WaypointFromId,
        WaypointToId = l.WaypointToId,
      }
    ));
    var linkToRemove = existingLinks.Except(mapEditorUpdateBusinessModel.WaypointLinks);
    _context.WaypointLinks.RemoveRange(linkToRemove
      .Select(l => new WaypointLink
      {
        Id = (int)l.Id!,
        RealmId = realmId,
        WaypointFromId = l.WaypointFromId,
        WaypointToId = l.WaypointToId,
      }
    ));
    await _context.SaveChangesAsync();
    return true;
  }
}