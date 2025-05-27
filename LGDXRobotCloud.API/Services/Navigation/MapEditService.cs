using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface IMapEditService
{
  Task<MapEditBusinessModel> GetMapEditAsync(int realmId);
  Task<bool> UpdateMapEditlAsync(int realmId, MapEditUpdateBusinessModel MapEditUpdateBusinessModel);
}

public class MapEditService(LgdxContext context) : IMapEditService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<MapEditBusinessModel> GetMapEditAsync(int realmId)
  {
    // Check if realm exists
    var realm = await _context.Realms.AsNoTracking()
      .Where(r => r.Id == realmId)
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();

    var waypoints = await _context.Waypoints.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .Select(w => new WaypointBusinessModel
      {
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
    return new MapEditBusinessModel
    {
      Waypoints = waypoints,
      WaypointLinks = waypointLinks,
    };
  }

  public async Task<bool> UpdateMapEditlAsync(int realmId, MapEditUpdateBusinessModel MapEditUpdateBusinessModel)
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
    var linkToAdd = MapEditUpdateBusinessModel.WaypointLinks.Except(existingLinks);
    _context.WaypointLinks.AddRange(linkToAdd
      .Select(l => new WaypointLink
      {
        Id = (int)l.Id!,
        RealmId = realmId,
        WaypointFromId = l.WaypointFromId,
        WaypointToId = l.WaypointToId,
      }
    ));
    var linkToRemove = existingLinks.Except(MapEditUpdateBusinessModel.WaypointLinks);
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