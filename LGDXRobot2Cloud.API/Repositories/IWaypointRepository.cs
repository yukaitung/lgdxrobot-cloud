using LGDXRobot2Cloud.API.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IWaypointRepository
  {
    Task<IEnumerable<Waypoint>> GetWaypointsAsync();
    Task AddWaypointAsync(Waypoint waypoint);
    Task<bool> SaveChangesAsync();
  }
}