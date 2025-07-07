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
[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.ApiKeyOrCertificateScheme)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class FlowsController(
    IOptionsSnapshot<LgdxRobotCloudConfiguration> lgdxRobotCloudConfiguration,
    IFlowService flowService
  ) : ControllerBase
{
  private readonly LgdxRobotCloudConfiguration _lgdxRobotCloudConfiguration = lgdxRobotCloudConfiguration.Value;
  private readonly IFlowService _flowService = flowService;

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<FlowListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<FlowListDto>>> GetFlows(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobotCloudConfiguration.ApiMaxPageSize) ? _lgdxRobotCloudConfiguration.ApiMaxPageSize : pageSize;
    var (flows, PaginationHelper) = await _flowService.GetFlowsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(flows.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<FlowSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<FlowSearchDto>>> SearchFlows(string? name)
  {
    var flows = await _flowService.SearchFlowsAsync(name);
    return Ok(flows.ToDto());
  }

  [HttpGet("{id}", Name = "GetFlow")]
  [ProducesResponseType(typeof(FlowDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<FlowDto>> GetFlow(int id)
  {
    var flow = await _flowService.GetFlowAsync(id);
    return Ok(flow.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(FlowDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateFlow(FlowCreateDto flowCreateDto)
  {
    var flow = await _flowService.CreateFlowAsync(flowCreateDto.ToBusinessModel());
    return CreatedAtAction(nameof(GetFlow), new { id = flow.Id }, flow.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateFlow(int id, FlowUpdateDto flowUpdateDto)
  {
    await _flowService.UpdateFlowAsync(id, flowUpdateDto.ToBusinessModel());
    return NoContent();
  }

  [HttpPost("{id}/TestDelete")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> TestDeleteFlow(int id)
  {
    await _flowService.TestDeleteFlowAsync(id);
    return Ok();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteFlow(int id)
  {
    await _flowService.TestDeleteFlowAsync(id);
    if(!await _flowService.DeleteFlowAsync(id))
    {
      return NotFound();
    }
    return NoContent();
  }
}