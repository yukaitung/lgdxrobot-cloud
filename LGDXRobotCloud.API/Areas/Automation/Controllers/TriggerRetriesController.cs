using LGDXRobotCloud.API.Authorisation;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.Models.Business.Automation;
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
public class TriggerRetriesController (
  IOptionsSnapshot<LgdxRobotCloudConfiguration> lgdxRobotCloudConfiguration,
  ITriggerRetryService triggerRetryService
) : ControllerBase
{
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = lgdxRobotCloudConfiguration.Value ?? throw new ArgumentNullException(nameof(lgdxRobotCloudConfiguration));
  private readonly ITriggerRetryService _triggerRetryService = triggerRetryService ?? throw new ArgumentNullException(nameof(triggerRetryService));
  
  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<TriggerRetryListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TriggerRetryListDto>>> GetTriggerRetries(int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobotCloudConfiguration.ApiMaxPageSize) ? _lgdxRobotCloudConfiguration.ApiMaxPageSize : pageSize;
    var (triggerRetries, PaginationHelper) = await _triggerRetryService.GetTriggerRetriesAsync(pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(triggerRetries.ToDto());
  }

  [HttpGet("{id}", Name = "GetTriggerRetry")]
  [ProducesResponseType(typeof(TriggerRetryDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<TriggerRetryDto>> GetTriggerRetry(int id)
  {
    var triggerRetry = await _triggerRetryService.GetTriggerRetryAsync(id);
    return Ok(triggerRetry.ToDto());
  }

  [HttpPost("{id}/Retry")]
  [ProducesResponseType(typeof(TriggerRetryDto), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<TriggerRetryDto>> RetryTriggerRetry(int id)
  {
    await _triggerRetryService.RetryTriggerRetryAsync(id);
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteTriggerRetry(int id)
  {
    if (await _triggerRetryService.DeleteTriggerRetryAsync(id))
    {
      return NoContent();
    }
    else
    {
      return NotFound();
    }
  }
}