using AutoMapper;
using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Setting.Controllers;

[ApiController]
[Area("Setting")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public class ApiKeysController(
  IApiKeyRepository apiKeyRepository,
  IMapper mapper,
  IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration) : ControllerBase
{
  private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository;
  private readonly IMapper _mapper = mapper;
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value;

  private static string GenerateApiKeys()
  {
    var bytes = RandomNumberGenerator.GetBytes(32);
    string base64String = Convert.ToBase64String(bytes)
      .Replace("+", "-")
      .Replace("/", "_");
    return "LGDX2" + base64String;
  }

  [HttpGet("")]
  public async Task<ActionResult<IEnumerable<ApiKeyDto>>> GetApiKeys(string? name, bool isThirdParty = false, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (apiKeys, PaginationHelper) = await _apiKeyRepository.GetApiKeysAsync(name, isThirdParty, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<ApiKeyDto>>(apiKeys));
  }

  [HttpGet("{id}", Name = "GetApiKey")]
  public async Task<ActionResult<ApiKeyDto>> GetApiKey(int id)
  {
    var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKey == null)
      return NotFound();
    return Ok(_mapper.Map<ApiKeyDto>(apiKey));
  }

  [HttpPost("")]
  public async Task<ActionResult> CreateApiKey(ApiKeyCreateDto apiKeyDto)
  {
    // Extra validation
    if (!apiKeyDto.IsThirdParty && !string.IsNullOrEmpty(apiKeyDto.Secret))
      return BadRequest("The Key field should be empty for LGDXRobot2 API Key.");
    // Generate LGDXRobot2 API Key
    if (!apiKeyDto.IsThirdParty)
      apiKeyDto.Secret = GenerateApiKeys();
    var apiKeyEntity = _mapper.Map<ApiKey>(apiKeyDto);
    await _apiKeyRepository.AddApiKeyAsync(apiKeyEntity);
    await _apiKeyRepository.SaveChangesAsync();
    var returnApiKey = _mapper.Map<ApiKeyDto>(apiKeyEntity);
    return CreatedAtRoute(nameof(GetApiKey), new { id = returnApiKey.Id }, returnApiKey);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateApiKey(int id, ApiKeyUpdateDto apiKeyDto)
  {
    var apiKeyEntity = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKeyEntity == null)
      return NotFound();
    _mapper.Map(apiKeyDto, apiKeyEntity);
    apiKeyEntity.UpdatedAt = DateTime.UtcNow;
    await _apiKeyRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteApiKey(int id)
  {
    var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKey == null)
      return NotFound();
    _apiKeyRepository.DeleteApiKey(apiKey);
    await _apiKeyRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpGet("{id}/secret")]
  public async Task<ActionResult<ApiKeySecretDto>> GetApiKeySecret(int id)
  {
    var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKey == null)
      return NotFound();
    return Ok(_mapper.Map<ApiKeySecretDto>(apiKey));
  }

  [HttpPut("{id}/secret")]
  public async Task<ActionResult> UpdateApiKeySecret(int id, ApiKeySecretDto apiKeyDto)
  {
    var apiKeyEntity = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKeyEntity == null)
      return NotFound();
    // Extra validation
    if (!apiKeyEntity.IsThirdParty && !string.IsNullOrEmpty(apiKeyDto.Secret))
      return BadRequest("The LGDXRobot2 API Key cannot be changed.");
    _mapper.Map(apiKeyDto, apiKeyEntity);
    apiKeyEntity.UpdatedAt = DateTime.UtcNow;
    await _apiKeyRepository.SaveChangesAsync();
    return NoContent();
  }
}