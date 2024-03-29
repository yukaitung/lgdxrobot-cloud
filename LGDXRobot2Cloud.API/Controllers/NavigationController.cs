using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.API.DbContexts;
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
    private readonly IApiKeyLocationRepository _apiKeyLocationRepository;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly ITriggerRepository _triggerRepository;
    private readonly IWaypointRepository _waypointRepository;
    private readonly IMapper _mapper;
    private readonly int maxPageSize = 100;

    public NavigationController(IApiKeyLocationRepository apiKeyLocationRepository,
      IApiKeyRepository apiKeyRepository,
      IProgressRepository progressRepository,
      ITriggerRepository triggerRepository,
      IWaypointRepository waypointRepository,
      IMapper mapper)
    {
      _apiKeyLocationRepository = apiKeyLocationRepository ?? throw new ArgumentNullException(nameof(apiKeyLocationRepository));
      _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
      _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
      _triggerRepository = triggerRepository ?? throw new ArgumentNullException(nameof(triggerRepository));
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
    ** Trigger
    */
    [HttpGet("triggers")]
    public async Task<ActionResult<IEnumerable<TriggerDto>>> GetTriggers(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (triggers, paginationMetadata) = await _triggerRepository.GetTriggersAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
      return Ok(_mapper.Map<IEnumerable<TriggerDto>>(triggers));
    }

    [HttpGet("triggers/{id}", Name = "GetTrigger")]
    public async Task<ActionResult<TriggerDto>> GetTrigger(int id)
    {
      var trigger = await _triggerRepository.GetTriggerAsync(id);
      if(trigger == null)
        return NotFound();
      return Ok(_mapper.Map<TriggerDto>(trigger));
    }

    [HttpPost("triggers")]
    public async Task<ActionResult> CreateTrigger(TriggerCreateDto trigger)
    {
      var addTrigger = _mapper.Map<Trigger>(trigger);
      var apiKeyLocation = await _apiKeyLocationRepository.GetApiKeyLocationAsync(trigger.ApiKeyLocationStr);
      if(apiKeyLocation == null)
        return BadRequest("The API key location is invalid.");
      addTrigger.ApiKeyLocationId = apiKeyLocation.Id;
      var apiKey = await _apiKeyRepository.GetApiKeyAsync(trigger.ApiKeyId);
      if(apiKey == null)
        return BadRequest("The API key is invalid.");
      if(!apiKey.isThirdParty)
        return BadRequest("Only accept third party API key.");
      await _triggerRepository.AddTriggerAsync(addTrigger);
      await _triggerRepository.SaveChangesAsync();
      var returnTrigger = _mapper.Map<TriggerDto>(addTrigger);
      return CreatedAtAction(nameof(GetTrigger), new {id = returnTrigger.Id}, returnTrigger);
    }

    [HttpPut("triggers/{id}")]
    public async Task<ActionResult> UpdateTrigger(int id, TriggerCreateDto trigger)
    {
      var triggerEntity = await _triggerRepository.GetTriggerAsync(id);
      if(triggerEntity == null)
        return NotFound();
      _mapper.Map(trigger, triggerEntity);
      var apiKeyLocation = await _apiKeyLocationRepository.GetApiKeyLocationAsync(trigger.ApiKeyLocationStr);
      if(apiKeyLocation == null)
        return BadRequest("The API key location is invalid.");
      triggerEntity.ApiKeyLocationId = apiKeyLocation.Id;
      var apiKey = await _apiKeyRepository.GetApiKeyAsync(trigger.ApiKeyId);
      if(apiKey == null)
        return BadRequest("The API key is invalid.");
      if(!apiKey.isThirdParty)
        return BadRequest("Only accept third party API key.");
      triggerEntity.UpdatedAt = DateTime.UtcNow;
      await _triggerRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("triggers/{id}")]
    public async Task<ActionResult> DeleteTrigger(int id)
    {
      var trigger = await _triggerRepository.GetTriggerAsync(id);
      if(trigger == null)
        return NotFound();
      _triggerRepository.DeleteTrigger(trigger);
      await _triggerRepository.SaveChangesAsync();
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