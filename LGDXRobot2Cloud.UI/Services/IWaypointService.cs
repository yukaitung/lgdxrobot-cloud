using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IWaypointService
  {
    Task<(IEnumerable<Waypoint>?, PaginationMetadata?)> GetWaypointsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<Waypoint?> GetWaypointAsync(int waypointId);
    Task<Waypoint?> AddWaypointAsync(WaypointCreateDto waypoint);
    Task<bool> UpdateWaypointAsync(int waypointId, WaypointCreateDto waypoint);
    Task<bool> DeleteWaypointAsync(int waypointId);
  }
}