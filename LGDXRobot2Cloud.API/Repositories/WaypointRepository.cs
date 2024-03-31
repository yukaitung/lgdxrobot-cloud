using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class WaypointRepository : IWaypointRepository
  {
    private readonly LgdxContext _context;

    public WaypointRepository(LgdxContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Waypoint>> GetWaypointsAsync()
    {
      return await _context.Waypoints.OrderBy(w => w.Id).ToListAsync();
    }

    public async Task<Waypoint?> GetWaypointAsync(int waypointId)
    {
      return await _context.Waypoints.Where(w => w.Id == waypointId).FirstOrDefaultAsync();
    }

    public async Task<bool> WaypointExistsAsync(int waypointId)
    {
      return await _context.Waypoints.AnyAsync(w => w.Id == waypointId);
    }

    public async Task AddWaypointAsync(Waypoint waypoint)
    {
      await _context.Waypoints.AddAsync(waypoint);
    }

    public void DeleteWaypoint(Waypoint waypoint)
    {
      _context.Waypoints.Remove(waypoint);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }

    public async Task<Dictionary<int, Waypoint>> GetWaypointsDictFromListAsync(IEnumerable<int> waypointIds)
    {
      return await _context.Waypoints.Where(w => waypointIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w);
    }
  }
}