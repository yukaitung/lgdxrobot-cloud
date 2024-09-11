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

public class TriggersController(
  IApiKeyRepository _apiKeyRepository,
  IMapper mapper,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
  ITriggerRepository triggerRepository) : ControllerBase
{
  private readonly IApiKeyRepository _apiKeyRepository = _apiKeyRepository;
  private readonly IMapper _mapper = mapper;
  private readonly ITriggerRepository _triggerRepository = triggerRepository;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<TriggerListDto>>> GetTriggers(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (triggers, PaginationHelper) = await _triggerRepository.GetTriggersAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<TriggerListDto>>(triggers));
  }

  [HttpGet("{id}", Name = "GetTrigger")]
  public async Task<ActionResult<TriggerDto>> GetTrigger(int id)
  {
    var trigger = await _triggerRepository.GetTriggerAsync(id);
    if (trigger == null)
      return NotFound();
    return Ok(_mapper.Map<TriggerDto>(trigger));
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateTrigger(TriggerCreateDto triggerDto)
  {
    var triggerEntity = _mapper.Map<Trigger>(triggerDto);
    if (triggerDto.ApiKeyId != null)
    {
      var apiKey = await _apiKeyRepository.GetApiKeyAsync((int)triggerDto.ApiKeyId);
      if (apiKey == null)
        return BadRequest($"The API Key Id {triggerDto.ApiKeyId} is invalid.");
      if (!apiKey.IsThirdParty)
        return BadRequest("Only third party API key is allowed.");
    }
    await _triggerRepository.AddTriggerAsync(triggerEntity);
    await _triggerRepository.SaveChangesAsync();
    var returnTrigger = _mapper.Map<TriggerDto>(triggerEntity);
    return CreatedAtAction(nameof(GetTrigger), new { id = returnTrigger.Id }, returnTrigger);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateTrigger(int id, TriggerUpdateDto triggerDto)
  {
    var triggerEntity = await _triggerRepository.GetTriggerAsync(id);
    if (triggerEntity == null)
      return NotFound();
    _mapper.Map(triggerDto, triggerEntity);
    if (triggerDto.ApiKeyId != null)
    {
      var apiKey = await _apiKeyRepository.GetApiKeyAsync((int)triggerDto.ApiKeyId);
      if (apiKey == null)
        return BadRequest($"The API Key Id {triggerDto.ApiKeyId} is invalid.");
      if (!apiKey.IsThirdParty)
        return BadRequest("Only third party API key is allowed.");
    }
    triggerEntity.UpdatedAt = DateTime.UtcNow;
    await _triggerRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteTrigger(int id)
  {
    var trigger = await _triggerRepository.GetTriggerAsync(id);
    if (trigger == null)
      return NotFound();
    _triggerRepository.DeleteTrigger(trigger);
    await _triggerRepository.SaveChangesAsync();
    return NoContent();
  }
}