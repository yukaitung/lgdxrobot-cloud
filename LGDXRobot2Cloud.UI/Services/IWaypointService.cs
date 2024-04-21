using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IWaypointService
  {
    Task<(IEnumerable<WaypointBlazor>?, PaginationMetadata?)> GetWaypointsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<WaypointBlazor?> GetWaypointAsync(int waypointId);
    Task<WaypointBlazor?> AddWaypointAsync(WaypointCreateDto waypoint);
    Task<bool> UpdateWaypointAsync(int waypointId, WaypointUpdateDto waypoint);
    Task<bool> DeleteWaypointAsync(int waypointId);
  }
}