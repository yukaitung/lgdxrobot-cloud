using LGDXRobotCloud.API.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Requests;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class RobotsController(
    IRobotService robotService,
    IOnlineRobotsService OnlineRobotsService,
    IOptionsSnapshot<LgdxRobot2Configuration> options
  ) : ControllerBase
{
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService ?? throw new ArgumentNullException(nameof(OnlineRobotsService));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = options.Value ?? throw new ArgumentNullException(nameof(options));  

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<RobotListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RobotListDto>>> GetRobots(int? realmId, string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (robots, paginationHelper) = await _robotService.GetRobotsAsync(realmId, name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationHelper));
    return Ok(robots.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<RobotSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<RobotSearchDto>>> SearchRobots(int realmId, string? name, Guid? robotId)
  {
    var robots = await _robotService.SearchRobotsAsync(realmId, name, robotId);
    return Ok(robots.ToDto());
  }

  [HttpGet("{id}", Name = "GetRobot")]
  [ProducesResponseType(typeof(RobotDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RobotDto>> GetRobot(Guid id)
  {
    var robot = await _robotService.GetRobotAsync(id);
    return Ok(robot.ToDto());
  }

  [HttpPost(Name = "CreateRobot")]
  [ProducesResponseType(typeof(RobotCertificateIssueDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateRobot(RobotCreateDto robotCreateDto)
  {
    var robot = await _robotService.CreateRobotAsync(robotCreateDto.ToBusinessModel());
    return CreatedAtAction(nameof(CreateRobot), robot.ToDto());
  }

  [HttpPatch("{id}/EmergencyStop")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> SetSoftwareEmergencyStopAsync(Guid id, EnableDto data)
  {
    if (await _onlineRobotsService.SetSoftwareEmergencyStopAsync(id, data.Enable))
    {
      return NoContent();
    }
    ModelState.AddModelError(nameof(id), "Robot is offline or not found.");
    return ValidationProblem();
  }

  [HttpPatch("{id}/PauseTaskAssigement")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> SetPauseTaskAssigementAsync(Guid id, EnableDto data)
  {
    if (await _onlineRobotsService.SetPauseTaskAssigementAsync(id, data.Enable))
    {
      return NoContent();
    }
    ModelState.AddModelError(nameof(id), "Robot is offline or not found.");
    return ValidationProblem();
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRobot(Guid id, RobotUpdateDto robotUpdateDto)
  {
    if (!await _robotService.UpdateRobotAsync(id, robotUpdateDto.ToBusinessModel()))
    {
      return NotFound();
    }
    return NoContent();
  }

  [HttpPut("{id}/Chassis")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateRobotChassisInfo(Guid id, RobotChassisInfoUpdateDto robotChassisInfoUpdateDto)
  {
    if (!await _robotService.UpdateRobotChassisInfoAsync(id, robotChassisInfoUpdateDto.ToBusinessModel()))
    {
      return NotFound();
    }
    return NoContent();
  }

  [HttpPost("{id}/TestDelete")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> TestDeleteRobot(Guid id)
  {
    await _robotService.TestDeleteRobotAsync(id);
    return Ok();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteRobot(Guid id)
  {
    await _robotService.TestDeleteRobotAsync(id);
    if (!await _robotService.DeleteRobotAsync(id))
    {
      return NotFound();
    }
    return NoContent();
  }
}
