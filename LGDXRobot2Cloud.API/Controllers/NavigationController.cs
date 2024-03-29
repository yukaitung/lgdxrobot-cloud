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
    private readonly IProgressRepository _progressRepository;
    private readonly IWaypointRepository _waypointRepository;
    private readonly IMapper _mapper;

    public NavigationController(IProgressRepository progressRepository,
      IWaypointRepository waypointRepository,
      IMapper mapper)
    {
      _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
      _waypointRepository = waypointRepository ?? throw new ArgumentNullException(nameof(waypointRepository));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /*
    ** Progress
    */
    [HttpGet("progresses")]
    public async Task<ActionResult<IEnumerable<ProgressDto>>> GetProgresses()
    {
      var progresses = await _progressRepository.GetProgressesAsync();
      return Ok(_mapper.Map<IEnumerable<ProgressDto>>(progresses));
    }

    [HttpGet("progresses/{id}", Name = "GetProgress")]
    public async Task<ActionResult<Progress>> GetProgress(int id)
    {
      var progress = await _progressRepository.GetProgressAsync(id);
      if(progress == null)
        return NotFound();
      return Ok(_mapper.Map<ProgressDto>(progress));
    }

    [HttpPost("progresses")]
    public async Task<ActionResult> CreateProgress(ProgressCreateDto progress)
    {
      var addProgress = _mapper.Map<Progress>(progress);
      await _progressRepository.AddProgressAsync(addProgress);
      await _progressRepository.SaveChangesAsync();
      var returnProgress = _mapper.Map<ProgressDto>(addProgress);
      return CreatedAtRoute(nameof(GetProgress), new {id = returnProgress.Id}, returnProgress);
    }

    [HttpPut("progresses/{id}")]
    public async Task<ActionResult> UpdateProgress(int id, ProgressCreateDto progress)
    {
      var progressEntity = await _progressRepository.GetProgressAsync(id);
      if(progressEntity == null)
        return NotFound();
      if(progressEntity.System)
        return BadRequest("Cannot update system defined progress.");
      _mapper.Map(progress, progressEntity);
      progressEntity.UpdatedAt = DateTime.UtcNow;
      await _progressRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("progresses/{id}")]
    public async Task<ActionResult> DeleteProgress(int id)
    {
      var progress = await _progressRepository.GetProgressAsync(id);
      if(progress == null)
        return NotFound();
      if(progress.System)
        return BadRequest("Cannot delete system defined progress.");
      _progressRepository.DeleteProgress(progress);
      await _progressRepository.SaveChangesAsync();
      return NoContent();
    }

    /*
    ** Waypoint
    */
    [HttpGet("waypoints")]
    public async Task<ActionResult<IEnumerable<WaypointDto>>> GetWaypoints()
    {
      var waypoints = await _waypointRepository.GetWaypointsAsync();
      return Ok(_mapper.Map<IEnumerable<WaypointDto>>(waypoints));
    }

    [HttpGet("waypoints/{id}", Name = "GetWaypoint")]
    public async Task<ActionResult<WaypointDto>> GetWaypoint(int id)
    {
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