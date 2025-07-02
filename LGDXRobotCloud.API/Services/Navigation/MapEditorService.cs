using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Navigation;

public record WaypointsTraffic
{
  public Dictionary<int, Waypoint> Waypoints { get; set; } = null!;
  public Dictionary<int, HashSet<int>> WaypointTraffics { get; set; } = null!;
}

public interface IMapEditorService
{
  Task<MapEditorBusinessModel> GetMapAsync(int realmId);
  Task<bool> UpdateMapAsync(int realmId, MapEditorUpdateBusinessModel MapEditUpdateBusinessModel);

  Task<WaypointsTraffic> GetWaypointTrafficAsync(int realmId);
}

public class MapEditorService(
    IActivityLogService activityLogService,
    IMemoryCache memoryCache,
    LgdxContext context
  ) : IMapEditorService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
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
    _memoryCache.Remove($"MapEditorService_InternalWaypointsTraffic_{realmId}");

    // Sort traffics
    var inputWaypointTraffics = mapEditorUpdateBusinessModel.WaypointTraffics
      .OrderBy(w => w.WaypointFromId)
      .ThenBy(w => w.WaypointToId)
      .ToList();

    // Get all traffics
    var databaseWaypointTraffics = await _context.WaypointTraffics.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .OrderBy(w => w.WaypointFromId)
      .ThenBy(w => w.WaypointToId)
      .ToListAsync();

    // Get traffic to add and remove
    List<WaypointTraffic> trafficsToAdd = [];
    int i = 0;
    int j = 0;
    while (i < inputWaypointTraffics.Count && j < databaseWaypointTraffics.Count)
    {
      // Same traffic, remove from list
      if (inputWaypointTraffics[i].WaypointFromId == databaseWaypointTraffics[j].WaypointFromId
        && inputWaypointTraffics[i].WaypointToId == databaseWaypointTraffics[j].WaypointToId)
      {
        inputWaypointTraffics.RemoveAt(i);
        databaseWaypointTraffics.RemoveAt(j);
      }
      else
      {
        if (inputWaypointTraffics[i].WaypointFromId > databaseWaypointTraffics[j].WaypointFromId
          && inputWaypointTraffics[i].WaypointToId > databaseWaypointTraffics[j].WaypointToId)
        {
          j++;
        }
        else
        {
          i++;
        }
      }
    }

    // inputWaypointTraffics contains the traffics to add
    await _context.WaypointTraffics.AddRangeAsync(inputWaypointTraffics.Select(w => new WaypointTraffic
    {
      RealmId = realmId,
      WaypointFromId = w.WaypointFromId,
      WaypointToId = w.WaypointToId,
    }));
    // databaseWaypointTraffics contains the traffics to remove
    _context.WaypointTraffics.RemoveRange(databaseWaypointTraffics);
    await _context.SaveChangesAsync();

    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(Realm),
      EntityId = realmId.ToString(),
      Action = ActivityAction.RealmTrafficUpdated,
    });

    return true;
  }

  public async Task<WaypointsTraffic> GetWaypointTrafficAsync(int realmId)
  {
    if (_memoryCache.TryGetValue($"MapEditorService_InternalWaypointsTraffic_{realmId}", out WaypointsTraffic? t))
    {
      return t ?? new();
    }
    
    var waypoints = await _context.Waypoints.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .ToListAsync();
    var waypointTraffics = await _context.WaypointTraffics.AsNoTracking()
      .Where(w => w.RealmId == realmId)
      .ToListAsync();

    var waypointsDict = waypoints.ToDictionary(w => w.Id);
    var waypointTrafficsDict = new Dictionary<int, HashSet<int>>();
    foreach (var traffic in waypointTraffics)
    {
      if (waypointTrafficsDict.TryGetValue(traffic.WaypointFromId, out HashSet<int>? neighbors))
      {
        neighbors.Add(traffic.WaypointToId);
        waypointTrafficsDict[traffic.WaypointFromId] = neighbors;
      }
      else
      {
        waypointTrafficsDict[traffic.WaypointFromId] = [traffic.WaypointToId];
      }
    }

    var internalWaypointsTraffic = new WaypointsTraffic
    {
      Waypoints = waypointsDict,
      WaypointTraffics = waypointTrafficsDict,
    };
    _memoryCache.Set($"MapEditorService_InternalWaypointsTraffic_{realmId}", internalWaypointsTraffic);
    return internalWaypointsTraffic;
  }
}