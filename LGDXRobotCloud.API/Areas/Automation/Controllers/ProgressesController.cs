using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobotCloud.API.Areas.Automation.Controllers;

[ApiController]
[Area("Automation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class ProgressesController(
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IProgressService progressService
  ) : ControllerBase
{
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));
  private readonly IProgressService _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<ProgressDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ProgressDto>>> GetProgresses(string? name, int pageNumber = 1, int pageSize = 10, bool system = false)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (progresses, PaginationHelper) = await _progressService.GetProgressesAsync(name, pageNumber, pageSize, system);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(progresses.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<ProgressSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ProgressSearchDto>>> SearchProgresses(string? name, bool reserved = false)
  {
    var progresses = await _progressService.SearchProgressesAsync(name, reserved);
    return Ok(progresses.ToDto());
  }

  [HttpGet("{id}", Name = "GetProgress")]
  [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ProgressDto>> GetProgress(int id)
  {
    var progress = await _progressService.GetProgressAsync(id);
    return Ok(progress.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status201Created)]
  public async Task<ActionResult> CreateProgress(ProgressCreateDto progressCreateDto)
  {
    var progress = await _progressService.CreateProgressAsync(progressCreateDto.ToBusinessModel());
    return CreatedAtRoute(nameof(GetProgress), new { id = progress.Id }, progress.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateProgress(int id, ProgressUpdateDto progressUpdateDto)
  {
    await _progressService.UpdateProgressAsync(id, progressUpdateDto.ToBusinessModel());
    return NoContent();
  }

  [HttpPost("{id}/TestDelete")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> TestDeleteProgress(int id)
  {
    await _progressService.TestDeleteProgressAsync(id);
    return Ok();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteProgress(int id)
  {
    await _progressService.TestDeleteProgressAsync(id);
    await _progressService.DeleteProgressAsync(id);
    return NoContent();
  }
}