using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Models;
using LGDXRobot2Cloud.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class NavigationController : ControllerBase
  {
    private readonly IWaypointRepository _waypointRepository;

    public NavigationController(IWaypointRepository waypointRepository)
    {
      _waypointRepository = waypointRepository ?? throw new ArgumentNullException(nameof(waypointRepository));
    }

    [HttpGet("waypoints")]
    public async Task<ActionResult<IEnumerable<WaypointDto>>> GetWaypoints()
    {
      var waypoints = await _waypointRepository.GetWaypointsAsync();
      return Ok(waypoints);
    }

    [HttpPost("waypoints")]
    public async Task<ActionResult> CreateWaypoint(WaypointCreateDto waypoint)
    {
      Waypoint w = new Waypoint{
        Name = waypoint.Name,
        X = waypoint.X,
        Y = waypoint.Y,
        W = waypoint.W
      };
      await _waypointRepository.AddWaypointAsync(w);
      await _waypointRepository.SaveChangesAsync();
      return NoContent();
    }
  }
}