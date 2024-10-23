using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class WaypointsController(
  IMapper mapper,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
  IWaypointRepository waypointRepository) : ControllerBase
{
  private readonly IMapper _mapper = mapper;
  private readonly IWaypointRepository _waypointRepository = waypointRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<WaypointDto>>> GetWaypoints(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (waypoints, PaginationHelper) = await _waypointRepository.GetWaypointsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<WaypointDto>>(waypoints));
  }

  [HttpGet("{id}", Name = "GetWaypoint")]
  public async Task<ActionResult<WaypointDto>> GetWaypoint(int id)
  {
    var waypoint = await _waypointRepository.GetWaypointAsync(id);
    if (waypoint == null)
      return NotFound();
    return Ok(_mapper.Map<WaypointDto>(waypoint));
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateWaypoint(WaypointCreateDto waypointDto)
  {
    var waypointEntity = _mapper.Map<Waypoint>(waypointDto);
    await _waypointRepository.AddWaypointAsync(waypointEntity);
    await _waypointRepository.SaveChangesAsync();
    var returnWaypoint = _mapper.Map<WaypointDto>(waypointEntity);
    return CreatedAtRoute(nameof(GetWaypoint), new { id = returnWaypoint.Id }, returnWaypoint);
  }

  [HttpPut("{id}")]
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

  [HttpDelete("{id}")]
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