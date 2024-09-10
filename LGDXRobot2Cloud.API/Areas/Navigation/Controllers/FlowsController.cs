using AutoMapper;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Navigation.Controllers;

[ApiController]
[Area("Navigation")]
[Route("[area]/[controller]")]
public class FlowsController(
  IFlowRepository flowRepository,
  IMapper mapper,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
  IProgressRepository progressRepository,
  ITriggerRepository triggerRepository) : ControllerBase
{
  private readonly IFlowRepository _flowRepository = flowRepository;
  private readonly IMapper _mapper = mapper;
  private readonly IProgressRepository _progressRepository = progressRepository;
  private readonly ITriggerRepository _triggerRepository = triggerRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<FlowListDto>>> GetFlows(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (flows, PaginationHelper) = await _flowRepository.GetFlowsAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<FlowListDto>>(flows));
  }

  [HttpGet("{id}", Name = "GetFlow")]
  public async Task<ActionResult<FlowDto>> GetFlow(int id)
  {
    var flow = await _flowRepository.GetFlowAsync(id);
    if (flow == null)
      return NotFound();
    return Ok(_mapper.Map<FlowDto>(flow));
  }

  private async Task<(Flow?, string)> IsValidFlow(FlowUpdateDto flowDto)
  {
    var flowEntity = _mapper.Map<Flow>(flowDto);
    HashSet<int> progressIds = [];
    HashSet<int> triggerIds = [];
    // Create a Set of Progress Ids and Trigger Ids for validation
    foreach (var detail in flowEntity.FlowDetails)
    {
      progressIds.Add(detail.ProgressId);
      if (detail.StartTriggerId != null)
        triggerIds.Add((int)detail.StartTriggerId);
      if (detail.EndTriggerId != null)
        triggerIds.Add((int)detail.EndTriggerId);
    }
    var progresses = await _progressRepository.GetProgressesDictFromListAsync(progressIds);
    var triggers = await _triggerRepository.GetTriggersDictFromListAsync(triggerIds);
    foreach (var detail in flowEntity.FlowDetails)
    {
      // Valide Progress and Trigger for each Detail
      if (progresses.TryGetValue(detail.ProgressId, out var progress))
      {
        if (progress.Reserved)
          return (null, $"The Progress ID: {progress.Id} is reserved.");
      }
      else
      {
        return (null, $"The Progress Id: {detail.ProgressId} is invalid.");
      }
      if (detail.StartTriggerId != null &&
          !triggers.ContainsKey((int)detail.StartTriggerId))
      {
        return (null, $"The Start Trigger Id: {detail.StartTriggerId} is invalid.");
      }
      if (detail.EndTriggerId != null &&
          !triggers.ContainsKey((int)detail.EndTriggerId))
      {
        return (null, $"The End Trigger Id: {detail.EndTriggerId} is invalid.");
      }
    }
    return (flowEntity, string.Empty);
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateFlow(FlowCreateDto flowDto)
  {
    var (flowEntity, error) = await IsValidFlow(_mapper.Map<FlowUpdateDto>(flowDto));
    if (flowEntity == null)
      return BadRequest(error);
    await _flowRepository.AddFlowAsync(flowEntity);
    await _flowRepository.SaveChangesAsync();
    var returnFlow = _mapper.Map<FlowDto>(flowEntity);
    return CreatedAtAction(nameof(GetFlow), new { id = returnFlow.Id }, returnFlow);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateFlow(int id, FlowUpdateDto flowDto)
  {
    var flowEntity = await _flowRepository.GetFlowAsync(id);
    if(flowEntity == null)
      return NotFound();
    // Ensure the input flow does not include Detail ID from other flow
    HashSet<int> detailIds = [];
    foreach(var detailId in flowEntity.FlowDetails.Select(d => d.Id))
      detailIds.Add(detailId);
    foreach(var detailId in flowDto.FlowDetails.Select(d => d.Id))
    {
      if(detailId != null && !detailIds.Contains((int)detailId))
        return BadRequest($"The Flow Detail ID {(int)detailId} is belongs to other Flow.");
    }
    var (newFlowEntity, error) = await IsValidFlow(flowDto);
    if (newFlowEntity == null)
      return BadRequest(error);
    _mapper.Map(flowDto, flowEntity);
    flowEntity.UpdatedAt = DateTime.UtcNow;
    await _flowRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
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