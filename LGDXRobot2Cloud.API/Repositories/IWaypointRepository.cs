using LGDXRobot2Cloud.API.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IWaypointRepository
  {
    Task<IEnumerable<Waypoint>> GetWaypointsAsync();
    Task<Waypoint?> GetWaypointAsync(int waypointId);
    Task<bool> WaypointExistsAsync(int waypointId);
    Task AddWaypointAsync(Waypoint waypoint);
    void DeleteWaypoint(Waypoint waypoint);
    Task<bool> SaveChangesAsync();

     // Specific Functions
    Task<Dictionary<int, Waypoint>> GetWaypointsDictFromListAsync(IEnumerable<int> waypointIds);
  }
}