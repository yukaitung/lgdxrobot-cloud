using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IWaypointRepository
  {
    Task<(IEnumerable<Waypoint>, PaginationHelper)> GetWaypointsAsync(string? name, int pageNumber, int pageSize);
    Task<Waypoint?> GetWaypointAsync(int waypointId);
    Task<bool> WaypointExistsAsync(int waypointId);
    Task AddWaypointAsync(Waypoint waypoint);
    void DeleteWaypoint(Waypoint waypoint);
    Task<bool> SaveChangesAsync();

     // Specific Functions
    Task<Dictionary<int, Waypoint>> GetWaypointsDictFromListAsync(IEnumerable<int> waypointIds);
  }
  
  public class WaypointRepository(LgdxContext context) : IWaypointRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<(IEnumerable<Waypoint>, PaginationHelper)> GetWaypointsAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.Waypoints as IQueryable<Waypoint>;
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
      var waypoints = await query.AsNoTracking()
        .OrderBy(a => a.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (waypoints, PaginationHelper);
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
      return await _context.Waypoints.AsNoTracking().Where(w => waypointIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w);
    }
  }
}