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

namespace LGDXRobot2Cloud.API.Areas.Automation.Controllers;

[ApiController]
[Area("Automation")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class FlowsController(
    IFlowRepository flowRepository,
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    IProgressRepository progressRepository,
    ITriggerRepository triggerRepository
  ) : ControllerBase
{
  private readonly IFlowRepository _flowRepository = flowRepository;
  private readonly IMapper _mapper = mapper;
  private readonly IProgressRepository _progressRepository = progressRepository;
  private readonly ITriggerRepository _triggerRepository = triggerRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<FlowListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<FlowListDto>>> GetFlows(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (flows, PaginationHelper) = await _flowRepository.GetFlowsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<FlowListDto>>(flows));
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<FlowSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<FlowSearchDto>>> SearchProgresses(string name)
  {
    var progresses = await _flowRepository.SearchFlowsAsync(name);
    return Ok(_mapper.Map<IEnumerable<FlowSearchDto>>(progresses));
  }

  [HttpGet("{id}", Name = "GetFlow")]
  [ProducesResponseType(typeof(FlowDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<FlowDto>> GetFlow(int id)
  {
    var flow = await _flowRepository.GetFlowAsync(id);
    if (flow == null)
      return NotFound();
    return Ok(_mapper.Map<FlowDto>(flow));
  }

  private async Task<bool> ValidateFlow(HashSet<int> progressIds, HashSet<int> triggerIds)
  {
    bool isValid = true;
    var progresses = await _progressRepository.GetProgressesDictFromListAsync(progressIds);
    foreach (var progressId in progressIds)
    {
      if (progresses.TryGetValue(progressId, out var progress))
      {
        if (progress.Reserved)
        {
          ModelState.AddModelError(nameof(FlowDetailDto.Progress), $"The Progress ID: {progressId} is reserved.");
          isValid = false;
        }
      }
      else
      {
        ModelState.AddModelError(nameof(FlowDetailDto.Progress), $"The Progress Id: {progressId} is invalid.");
        isValid = false;
      }
    }
    var triggers = await _triggerRepository.GetTriggersDictFromListAsync(triggerIds);
    foreach (var triggerId in triggerIds)
    {
      if (!triggers.TryGetValue(triggerId, out var trigger))
      {
        ModelState.AddModelError(nameof(FlowDetailDto.Trigger), $"The Trigger ID: {triggerId} is invalid.");
        isValid = false;
      }
    }
    return isValid;
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(FlowDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateFlow(FlowCreateDto flowCreateDto)
  {
    HashSet<int> progressIds = flowCreateDto.FlowDetails.Select(d => d.ProgressId).ToHashSet();
    HashSet<int> triggerIds = flowCreateDto.FlowDetails.Where(d => d.TriggerId != null).Select(d => d.TriggerId!.Value).ToHashSet();
    if(!await ValidateFlow(progressIds, triggerIds))
    {
      return ValidationProblem();
    }
    var flowEntity = _mapper.Map<Flow>(flowCreateDto);
    await _flowRepository.AddFlowAsync(flowEntity);
    await _flowRepository.SaveChangesAsync();
    var flowDto = _mapper.Map<FlowDto>(flowEntity);
    return CreatedAtAction(nameof(GetFlow), new { id = flowDto.Id }, flowDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateFlow(int id, FlowUpdateDto flowUpdateDto)
  {
    var flowEntity = await _flowRepository.GetFlowAsync(id);
    if(flowEntity == null)
      return NotFound();

    // Ensure the input flow does not include Detail ID from other flow
    var isValid = true;
    HashSet<int> dbDetailIds = flowEntity.FlowDetails.Select(d => d.Id).ToHashSet();
    foreach(var dtoDetailId in flowUpdateDto.FlowDetails.Where(d => d.Id != null).Select(d => d.Id))
    {
      if(!dbDetailIds.Contains((int)dtoDetailId!))
      {
        ModelState.AddModelError(nameof(FlowUpdateDto.FlowDetails), $"The Flow Detail ID {(int)dtoDetailId} is belongs to other Flow.");
        isValid = false;
      }
    }
    if (!isValid)
    {
      return ValidationProblem();
    }

    // Validate Flow
    HashSet<int> progressIds = flowUpdateDto.FlowDetails.Select(d => d.ProgressId).ToHashSet();
    HashSet<int> triggerIds = flowUpdateDto.FlowDetails.Where(d => d.TriggerId != null).Select(d => d.TriggerId!.Value).ToHashSet();
    if(!await ValidateFlow(progressIds, triggerIds))
    {
      return ValidationProblem();
    }

    _mapper.Map(flowUpdateDto, flowEntity);
    await _flowRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteFlow(int id)
  {
    var flow = await _flowRepository.GetFlowAsync(id);
    if (flow == null)
      return NotFound();
    _flowRepository.DeleteFlow(flow);
    await _flowRepository.SaveChangesAsync();
    return NoContent();
  }
}