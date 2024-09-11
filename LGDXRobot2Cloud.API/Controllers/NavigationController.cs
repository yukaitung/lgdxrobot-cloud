using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using LGDXRobot2Cloud.API.Services;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Utilities.Helpers;

namespace LGDXRobot2Cloud.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class NavigationController(
    IApiKeyRepository apiKeyRepository,
    IFlowRepository flowRepository,
    IProgressRepository progressRepository,
    IRobotRepository robotRepository,
    IAutoTaskRepository autoTaskRepository,
    ITriggerRepository triggerRepository,
    IWaypointRepository waypointRepository,
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IOnlineRobotsService onlineRobotsService,
    IMapper mapper) : ControllerBase
  {
    private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
    private readonly IFlowRepository _flowRepository = flowRepository ?? throw new ArgumentNullException(nameof(flowRepository));
    private readonly IProgressRepository _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
    private readonly IRobotRepository _robotRepository = robotRepository ?? throw new ArgumentNullException(nameof(robotRepository));
    private readonly IAutoTaskRepository _autoTaskRepository = autoTaskRepository ?? throw new ArgumentNullException(nameof(autoTaskRepository));
    private readonly ITriggerRepository _triggerRepository = triggerRepository ?? throw new ArgumentNullException(nameof(triggerRepository));
    private readonly IWaypointRepository _waypointRepository = waypointRepository ?? throw new ArgumentNullException(nameof(waypointRepository));
    private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService ?? throw new ArgumentNullException(nameof(autoTaskSchedulerService));
    private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly int maxPageSize = 100;


    /*
    ** Progress
    */
    

    /*
    ** Task
    */


    /*
    ** Trigger
    */
    

    /*
    ** Waypoint
    */
    [HttpGet("waypoints")]
    public async Task<ActionResult<IEnumerable<WaypointDto>>> GetWaypoints(string? name, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (waypoints, PaginationHelper) = await _waypointRepository.GetWaypointsAsync(name, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
      return Ok(_mapper.Map<IEnumerable<WaypointDto>>(waypoints));
    }

    [HttpGet("waypoints/{id}", Name = "GetWaypoint")]
    public async Task<ActionResult<WaypointDto>> GetWaypoint(int id)
    {
      var waypoint = await _waypointRepository.GetWaypointAsync(id);
      if (waypoint == null)
        return NotFound();
      return Ok(_mapper.Map<WaypointDto>(waypoint));
    }

    [HttpPost("waypoints")]
    public async Task<ActionResult> CreateWaypoint(WaypointCreateDto waypointDto)
    {
      var waypointEntity = _mapper.Map<Waypoint>(waypointDto);
      await _waypointRepository.AddWaypointAsync(waypointEntity);
      await _waypointRepository.SaveChangesAsync();
      var returnWaypoint = _mapper.Map<WaypointDto>(waypointEntity);
      return CreatedAtRoute(nameof(GetWaypoint), new { id = returnWaypoint.Id }, returnWaypoint);
    }

    [HttpPut("waypoints/{id}")]
    public async Task<ActionResult> UpdateWaypoint(int id, WaypointUpdateDto waypointDto)
    {
      var waypointEntity = await _waypointRepository.GetWaypointAsync(id);
      if (waypointEntity == null)
        return NotFound();
      _mapper.Map(waypointDto, waypointEntity);
      waypointEntity.UpdatedAt = DateTime.UtcNow;
      await _waypointRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("waypoints/{id}")]
    public async Task<ActionResult> DeleteWaypoint(int id)
    {
      var waypoint = await _waypointRepository.GetWaypointAsync(id);
      if (waypoint == null)
        return NotFound();
      _waypointRepository.DeleteWaypoint(waypoint);
      await _waypointRepository.SaveChangesAsync();
      return NoContent();
    }
  }
}