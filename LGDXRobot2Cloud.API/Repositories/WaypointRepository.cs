using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;
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

    public async Task<(IEnumerable<Waypoint>, PaginationMetadata)> GetWaypointsAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.Waypoints as IQueryable<Waypoint>;
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageSize, pageNumber);
      var waypoints = await query.OrderBy(a => a.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (waypoints, paginationMetadata);
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