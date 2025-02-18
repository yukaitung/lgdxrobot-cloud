using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobotCloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class WaypointsController(
    IWaypointService waypointService,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration
  ) : ControllerBase
{
  private readonly IWaypointService _waypointService = waypointService ?? throw new ArgumentNullException(nameof(waypointService));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<WaypointListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<WaypointListDto>>> GetWaypoints(int? realmId, string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (waypoints, PaginationHelper) = await _waypointService.GetWaypointsAsync(realmId, name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(waypoints.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<WaypointSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<WaypointSearchDto>>> SearchWaypoints(int realmId, string? name)
  {
    var waypoints = await _waypointService.SearchWaypointsAsync(realmId, name);
    return Ok(waypoints.ToDto());
  }

  [HttpGet("{id}", Name = "GetWaypoint")]
  [ProducesResponseType(typeof(WaypointDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<WaypointDto>> GetWaypoint(int id)
  {
    var waypoint = await _waypointService.GetWaypointAsync(id);
    return Ok(waypoint.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(WaypointDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateWaypoint(WaypointCreateDto waypointCreateDto)
  {
    var waypoint = await _waypointService.CreateWaypointAsync(waypointCreateDto.ToBusinessModel());
    return CreatedAtRoute(nameof(GetWaypoint), new { id = waypoint.Id }, waypoint.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateWaypoint(int id, WaypointUpdateDto waypointUpdateDto)
  {
    if (!await _waypointService.UpdateWaypointAsync(id, waypointUpdateDto.ToBusinessModel())) 
    {
      return NotFound();
    }
    return NoContent();
  }

  [HttpPost("{id}/TestDelete")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> TestDeleteWaypoint(int id)
  {
    await _waypointService.TestDeleteWaypointAsync(id);
    return Ok();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteWaypoint(int id)
  {
    await _waypointService.TestDeleteWaypointAsync(id);
    if (!await _waypointService.DeleteWaypointAsync(id))
    {
      return NotFound();
    }
    return NoContent();
  }
}