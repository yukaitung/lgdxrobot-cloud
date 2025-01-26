using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Services.Automation;
using LGDXRobot2Cloud.Data.Models.Business.Automation;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Automation.Controllers;

[ApiController]
[Area("Automation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class TriggersController(
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    ITriggerService triggerService
  ) : ControllerBase
{
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));
  private readonly ITriggerService _triggerService = triggerService ?? throw new ArgumentNullException(nameof(triggerService));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<TriggerListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TriggerListDto>>> GetTriggers(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
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