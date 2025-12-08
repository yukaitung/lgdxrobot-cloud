using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Requests;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobotCloud.API.Areas.Automation.Controllers;

[ApiController]
[Area("Automation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificateScheme)]
public class AutoTasksApiController(
    IAutoTaskService autoTaskService
  ) : ControllerBase
{
  private readonly IAutoTaskService _autoTaskService = autoTaskService ?? throw new ArgumentNullException(nameof(autoTaskService));

  [HttpPost("/AutoTaskNext")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> AutoTaskNext(AutoTaskNextDto autoTaskNextDto)
  {
    await _autoTaskService.AutoTaskNextApiAsync(autoTaskNextDto.RobotId, autoTaskNextDto.TaskId, autoTaskNextDto.NextToken);
    return NoContent();
  }

  [HttpPost("/AutoTaskAbort")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> AutoTaskAbort(AutoTaskNextDto autoTaskNextDto)
  {
    await _autoTaskService.AbortAutoTaskApiAsync(autoTaskNextDto.RobotId, autoTaskNextDto.TaskId, autoTaskNextDto.NextToken);
    return NoContent();
  }
}