using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Requests;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Constants;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobotCloud.API.Areas.Automation.Controllers;

[ApiController]
[Area("Automation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificateScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class AutoTasksController(
    IOptionsSnapshot<LgdxRobotCloudConfiguration> lgdxRobotCloudConfiguration,
    IAutoTaskService autoTaskService
  ) : ControllerBase
{
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = lgdxRobotCloudConfiguration.Value ?? throw new ArgumentNullException(nameof(lgdxRobotCloudConfiguration));
  private readonly IAutoTaskService _autoTaskService = autoTaskService ?? throw new ArgumentNullException(nameof(autoTaskService));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<AutoTaskListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<AutoTaskListDto>>> GetTasks(int? realmId, string? name, AutoTaskCatrgory? autoTaskCatrgory, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobotCloudConfiguration.ApiMaxPageSize) ? _lgdxRobotCloudConfiguration.ApiMaxPageSize : pageSize;
    var (tasks, PaginationHelper) = await _autoTaskService.GetAutoTasksAsync(realmId, name, autoTaskCatrgory, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(tasks.ToDto());
  }

  [HttpGet("{id}", Name = "GetTask")]
  [ProducesResponseType(typeof(AutoTaskDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<AutoTaskDto>> GetTask(int id)
  {
    var autoTask = await _autoTaskService.GetAutoTaskAsync(id);
    return Ok(autoTask.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(AutoTaskDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateTask(AutoTaskCreateDto autoTaskCreateDto)
  {
    var autoTask = await _autoTaskService.CreateAutoTaskAsync(autoTaskCreateDto.ToBusinessModel());
    return CreatedAtAction(nameof(GetTask), new { id = autoTask.Id }, autoTask.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateTask(int id, AutoTaskUpdateDto autoTaskUpdateDto)
  {
    await _autoTaskService.UpdateAutoTaskAsync(id, autoTaskUpdateDto.ToBusinessModel());
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteTask(int id)
  {
    await _autoTaskService.DeleteAutoTaskAsync(id);
    return NoContent();
  }

  [HttpPost("{id}/Abort")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> AbortTask(int id)
  {
    await _autoTaskService.AbortAutoTaskAsync(id);
    return NoContent();
  }

  [HttpGet("Statistics/{realmId}")]
  [ProducesResponseType(typeof(AutoTaskStatisticsDto), StatusCodes.Status200OK)]
  public async Task<ActionResult<AutoTaskStatisticsDto>> GetStatistics(int realmId)
  {
    var statistics = await _autoTaskService.GetAutoTaskStatisticsAsync(realmId);
    return Ok(statistics.ToDto());
  }

  [HttpGet("RobotCurrentTask/{robotId}")]
  [ProducesResponseType(typeof(AutoTaskListDto), StatusCodes.Status200OK)]
  public async Task<ActionResult<AutoTaskListDto?>> GetRobotCurrentTask(Guid robotId)
  {
    var autoTask = await _autoTaskService.GetRobotCurrentTaskAsync(robotId);
    return Ok(autoTask?.ToDto());
  }
}