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

    public async Task AddWaypointAsync(Waypoint waypoint)
    {
      await _context.Waypoints.AddAsync(waypoint);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }
  }
}