using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Data.Models.DTOs.V1.Commands;
using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobotCloud.API.Areas.Automation.Controllers;

[ApiController]
[Area("Automation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificationScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class TriggersController(
    IOptionsSnapshot<LgdxRobotCloudConfiguration> lgdxRobotCloudConfiguration,
    ITriggerService triggerService
  ) : ControllerBase
{
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = lgdxRobotCloudConfiguration.Value ?? throw new ArgumentNullException(nameof(lgdxRobotCloudConfiguration));
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<TriggerListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TriggerListDto>>> GetTriggers(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobotCloudConfiguration.ApiMaxPageSize) ? _lgdxRobotCloudConfiguration.ApiMaxPageSize : pageSize;
    var (triggers, PaginationHelper) = await _triggerService.GetTriggersAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(triggers.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<TriggerSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TriggerSearchDto>>> SearchTriggers(string? name)
  {
    var triggers = await _triggerService.SearchTriggersAsync(name);
    return Ok(triggers.ToDto());
  }

  [HttpGet("{id}", Name = "GetTrigger")]
  [ProducesResponseType(typeof(TriggerDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<TriggerDto>> GetTrigger(int id)
  {
    var trigger = await _triggerService.GetTriggerAsync(id);
    return Ok(trigger.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(TriggerDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateTrigger(TriggerCreateDto triggerCreateDto)
  {
    var trigger = await _triggerService.CreateTriggerAsync(triggerCreateDto.ToBusinessModel());
    return CreatedAtAction(nameof(GetTrigger), new { id = trigger.Id }, trigger.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateTrigger(int id, TriggerUpdateDto triggerUpdateDto)
  {
    if (!await _triggerService.UpdateTriggerAsync(id, triggerUpdateDto.ToBusinessModel()))
    {
      return NotFound();
    }
    return NoContent();
  }

  [HttpPost("{id}/TestDelete")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> TestDeleteTrigger(int id)
  {
    await _triggerService.TestDeleteTriggerAsync(id);
    return Ok();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteTrigger(int id)
  {
    await _triggerService.TestDeleteTriggerAsync(id);
    if (!await _triggerService.DeleteTriggerAsync(id))  
    {
      return NotFound();
    }
    return NoContent();
  }
}