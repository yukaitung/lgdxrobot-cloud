using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IWaypointRepository
  {
    Task<(IEnumerable<Waypoint>, PaginationMetadata)> GetWaypointsAsync(string? name, int pageNumber, int pageSize);
    Task<Waypoint?> GetWaypointAsync(int waypointId);
    Task<bool> WaypointExistsAsync(int waypointId);
    Task AddWaypointAsync(Waypoint waypoint);
    void DeleteWaypoint(Waypoint waypoint);
    Task<bool> SaveChangesAsync();

     // Specific Functions
    Task<Dictionary<int, Waypoint>> GetWaypointsDictFromListAsync(IEnumerable<int> waypointIds);
  }
}