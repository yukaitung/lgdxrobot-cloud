using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
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
    IApiKeyRepository apiKeyRepository,
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration,
    ITriggerRepository triggerRepository
  ) : ControllerBase
{
  private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly ITriggerRepository _triggerRepository = triggerRepository ?? throw new ArgumentNullException(nameof(triggerRepository));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<TriggerListDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TriggerListDto>>> GetTriggers(string? name, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (triggers, PaginationHelper) = await _triggerRepository.GetTriggersAsync(name, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<TriggerListDto>>(triggers));
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<ProgressSearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ProgressSearchDto>>> SearchProgresses(string name)
  {
    var progresses = await _triggerRepository.SearchTriggersAsync(name);
    return Ok(_mapper.Map<IEnumerable<ProgressSearchDto>>(progresses));
  }

  [HttpGet("{id}", Name = "GetTrigger")]
  [ProducesResponseType(typeof(TriggerDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<TriggerDto>> GetTrigger(int id)
  {
    var trigger = await _triggerRepository.GetTriggerAsync(id);
    if (trigger == null)
      return NotFound();
    return Ok(_mapper.Map<TriggerDto>(trigger));
  }

  [HttpPost("")]
  [ProducesResponseType(typeof(TriggerDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> CreateTrigger(TriggerCreateDto triggerCreateDto)
  {
    var triggerEntity = _mapper.Map<Data.Entities.Trigger>(triggerCreateDto);
    if (triggerCreateDto.ApiKeyId != null)
    {
      var apiKey = await _apiKeyRepository.GetApiKeyAsync((int)triggerCreateDto.ApiKeyId);
      if (apiKey == null)
      {
        ModelState.AddModelError(nameof(triggerCreateDto.ApiKeyId), $"The API Key Id {triggerCreateDto.ApiKeyId} is invalid.");
        return ValidationProblem();
      }
      else if (!apiKey.IsThirdParty)
      {
        ModelState.AddModelError(nameof(triggerCreateDto.ApiKeyId), "Only third party API key is allowed.");
        return ValidationProblem();
      }
    }
    await _triggerRepository.AddTriggerAsync(triggerEntity);
    await _triggerRepository.SaveChangesAsync();
    var returnTrigger = _mapper.Map<TriggerDto>(triggerEntity);
    return CreatedAtAction(nameof(GetTrigger), new { id = returnTrigger.Id }, returnTrigger);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateTrigger(int id, TriggerUpdateDto triggerUpdateDto)
  {
    var triggerEntity = await _triggerRepository.GetTriggerAsync(id);
    if (triggerEntity == null)
      return NotFound();
    if (triggerUpdateDto.ApiKeyId != null)
    {
      var apiKey = await _apiKeyRepository.GetApiKeyAsync((int)triggerUpdateDto.ApiKeyId);
      if (apiKey == null)
      {
        ModelState.AddModelError(nameof(triggerUpdateDto.ApiKeyId), $"The API Key Id {triggerUpdateDto.ApiKeyId} is invalid.");
        return ValidationProblem();
      }
      else if (!apiKey.IsThirdParty)
      {
        ModelState.AddModelError(nameof(triggerUpdateDto.ApiKeyId), "Only third party API key is allowed.");
        return ValidationProblem();
      }
    }
    _mapper.Map(triggerUpdateDto, triggerEntity);
    await _triggerRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
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