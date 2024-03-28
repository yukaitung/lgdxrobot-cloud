using AutoMapper;
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
    private readonly IMapper _mapper;

    public NavigationController(IWaypointRepository waypointRepository,
      IMapper mapper)
    {
      _waypointRepository = waypointRepository ?? throw new ArgumentNullException(nameof(waypointRepository));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet("waypoints")]
    public async Task<ActionResult<IEnumerable<WaypointDto>>> GetWaypoints()
    {
      var waypoints = await _waypointRepository.GetWaypointsAsync();
      return Ok(_mapper.Map<IEnumerable<WaypointDto>>(waypoints));
    }

    [HttpGet("waypoints/{id}", Name = "GetWaypoint")]
    public async Task<ActionResult<WaypointDto>> GetWaypoint(int id)
    {
      if(!await _waypointRepository.WaypointExistsAsync(id))
        return NotFound();
      var waypoint = await _waypointRepository.GetWaypointAsync(id);
      if(waypoint == null)
        return NotFound();
      return Ok(_mapper.Map<WaypointDto>(waypoint));
    }

    [HttpPost("waypoints")]
    public async Task<ActionResult> CreateWaypoint(WaypointCreateDto waypoint)
    {
      var addWaypoint = _mapper.Map<Waypoint>(waypoint);
      await _waypointRepository.AddWaypointAsync(addWaypoint);
      await _waypointRepository.SaveChangesAsync();
      var returnWaypoint = _mapper.Map<WaypointDto>(addWaypoint);
      return CreatedAtRoute(nameof(GetWaypoint), new {id = returnWaypoint.Id}, returnWaypoint);
    }

    [HttpPut("waypoints/{id}")]
    public async Task<ActionResult> UpdateWaypoint(int id, WaypointCreateDto waypoint)
    {
      var waypointEntity = await _waypointRepository.GetWaypointAsync(id);
      if(waypointEntity == null)
        return NotFound();
      _mapper.Map(waypoint, waypointEntity);
      waypointEntity.UpdatedAt = DateTime.UtcNow;
      await _waypointRepository.SaveChangesAsync();
      return NoContent(); 
    }

    [HttpDelete("waypoints/{id}")]
    public async Task<ActionResult> DeleteWaypoint(int id)
    {
      var waypoint = await _waypointRepository.GetWaypointAsync(id);
      if(waypoint == null)
        return NotFound();
      _waypointRepository.DeleteWaypoint(waypoint);
      await _waypointRepository.SaveChangesAsync();
      return NoContent();
    }
  }
}