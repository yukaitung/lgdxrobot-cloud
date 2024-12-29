using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
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
    IRealmRepository realmRepository,
    IWaypointRepository waypointRepository
  ) : ControllerBase
{
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly IRealmRepository _realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
  private readonly IWaypointRepository _waypointRepository = waypointRepository ?? throw new ArgumentNullException(nameof(waypointRepository));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<WaypointListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<WaypointListDto>>> GetWaypoints(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (waypoints, PaginationHelper) = await _waypointRepository.GetWaypointsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<WaypointListDto>>(waypoints));
  }

  [HttpGet("{id}", Name = "GetWaypoint")]
  [ProducesResponseType(typeof(RealmDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<WaypointDto>> GetWaypoint(int id)
  {
    var waypoint = await _waypointRepository.GetWaypointAsync(id);
    if (waypoint == null)
      return NotFound();
    return Ok(_mapper.Map<WaypointDto>(waypoint));
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(WaypointDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateWaypoint(WaypointCreateDto waypointCreateDto)
  {
    if (!await _realmRepository.IsRealmExistsAsync(waypointCreateDto.RealmId))
    {
      ModelState.AddModelError(nameof(waypointCreateDto.RealmId), "Realm does not exist.");
      return BadRequest(ModelState);
    }
    var waypointEntity = _mapper.Map<Waypoint>(waypointCreateDto);
    await _waypointRepository.AddWaypointAsync(waypointEntity);
    await _waypointRepository.SaveChangesAsync();
    var waypointDto = _mapper.Map<WaypointDto>(waypointEntity);
    return CreatedAtRoute(nameof(GetWaypoint), new { id = waypointDto.Id }, waypointDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateWaypoint(int id, WaypointUpdateDto waypointUpdateDto)
  {
    var waypointEntity = await _waypointRepository.GetWaypointAsync(id);
    if (waypointEntity == null)
      return NotFound();
    if (!await _realmRepository.IsRealmExistsAsync(waypointUpdateDto.RealmId))
    {
      ModelState.AddModelError(nameof(waypointUpdateDto.RealmId), "Realm does not exist.");
      return BadRequest(ModelState);
    }
    _mapper.Map(waypointUpdateDto, waypointEntity);
    await _waypointRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
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