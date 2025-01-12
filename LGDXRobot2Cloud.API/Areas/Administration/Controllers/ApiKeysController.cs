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
using System.Security.Cryptography;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class ApiKeysController(
    IApiKeyRepository apiKeyRepository,
    IMapper mapper,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration
  ) : ControllerBase
{
  private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
  private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  private static string GenerateApiKeys()
  {
    var bytes = RandomNumberGenerator.GetBytes(32);
    string base64String = Convert.ToBase64String(bytes)
      .Replace("+", "-")
      .Replace("/", "_");
    return "LGDX2" + base64String;
  }

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<ApiKeyDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ApiKeyDto>>> GetApiKeys(string? name, bool isThirdParty = false, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (apiKeys, PaginationHelper) = await _apiKeyRepository.GetApiKeysAsync(name, isThirdParty, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(_mapper.Map<IEnumerable<ApiKeyDto>>(apiKeys));
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<ApiKeySearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ApiKeySearchDto>>> SearchApiKeys(string name)
  {
    var waypoints = await _apiKeyRepository.SearchApiKeysAsync(name);
    return Ok(_mapper.Map<IEnumerable<ApiKeySearchDto>>(waypoints));
  }

  [HttpGet("{id}", Name = "GetApiKey")]
  [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ApiKeyDto>> GetApiKey(int id)
  {
    var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKey == null)
      return NotFound();
    return Ok(_mapper.Map<ApiKeyDto>(apiKey));
  }

  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  public async Task<ActionResult> CreateApiKey(ApiKeyCreateDto apiKeyCreateDto)
  {
    // Generate LGDXRobot2 API Key
    if (!apiKeyCreateDto.IsThirdParty)
      apiKeyCreateDto.Secret = GenerateApiKeys();
    var apiKeyEntity = _mapper.Map<ApiKey>(apiKeyCreateDto);
    await _apiKeyRepository.AddApiKeyAsync(apiKeyEntity);
    await _apiKeyRepository.SaveChangesAsync();
    var apiKeyDto = _mapper.Map<ApiKeyDto>(apiKeyEntity);
    return CreatedAtRoute(nameof(GetApiKey), new { id = apiKeyDto.Id }, apiKeyDto);
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateApiKey(int id, ApiKeyUpdateDto apiKeyUpdateDto)
  {
    var apiKeyEntity = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKeyEntity == null)
      return NotFound();
    _mapper.Map(apiKeyUpdateDto, apiKeyEntity);
    await _apiKeyRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteApiKey(int id)
  {
    var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKey == null)
      return NotFound();
    _apiKeyRepository.DeleteApiKey(apiKey);
    await _apiKeyRepository.SaveChangesAsync();
    return NoContent();
  }

  [HttpGet("{id}/Secret")]
  [ProducesResponseType(typeof(ApiKeySecretDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ApiKeySecretDto>> GetApiKeySecret(int id)
  {
    var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKey == null)
      return NotFound();
    return Ok(_mapper.Map<ApiKeySecretDto>(apiKey));
  }

  [HttpPut("{id}/Secret")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateApiKeySecret(int id, ApiKeySecretUpdateDto apiKeySecretUpdateDto)
  {
    var apiKeyEntity = await _apiKeyRepository.GetApiKeyAsync(id);
    if (apiKeyEntity == null)
      return NotFound();
    // Extra validation
    if (!apiKeyEntity.IsThirdParty && !string.IsNullOrEmpty(apiKeySecretUpdateDto.Secret))
    {
      ModelState.AddModelError(nameof(apiKeySecretUpdateDto.Secret), "The LGDXRobot2 API Key cannot be changed.");
      return ValidationProblem();
    }
    _mapper.Map(apiKeySecretUpdateDto, apiKeyEntity);
    await _apiKeyRepository.SaveChangesAsync();
    return NoContent();
  }
}