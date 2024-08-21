using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace LGDXRobot2Cloud.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class SettingController(IApiKeyRepository apiKeyRepository,
    IMapper mapper) : ControllerBase
  {
    private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly int maxPageSize = 100;

    private static string GenerateApiKeys()
    {
      // https://www.camiloterevinto.com/post/simple-and-secure-api-keys-using-asp-net-core
      var bytes = RandomNumberGenerator.GetBytes(32);
      string base64String = Convert.ToBase64String(bytes)
        .Replace("+", "-")
        .Replace("/", "_");
      return "LGDX2" + base64String;
    }

    /*
    ** Api Key
    */
    [HttpGet("secret/apikeys")]
    public async Task<ActionResult<IEnumerable<ApiKeyDto>>> GetApiKeys(string? name, bool isThirdParty = false, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (apiKeys, PaginationHelper) = await _apiKeyRepository.GetApiKeysAsync(name, isThirdParty, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
      return Ok(_mapper.Map<IEnumerable<ApiKeyDto>>(apiKeys));
    }

    [HttpGet("secret/apikeys/{id}", Name = "GetApiKey")]
    public async Task<ActionResult<ApiKeyDto>> GetApiKey(int id)
    {
      var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
      if (apiKey == null)
        return NotFound();
      return Ok(_mapper.Map<ApiKeyDto>(apiKey));
    }

    [HttpPost("secret/apikeys")]
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

    [HttpPut("secret/apikeys/{id}")]
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

    [HttpDelete("secret/apikeys/{id}")]
    public async Task<ActionResult> DeleteApiKey(int id)
    {
      var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
      if (apiKey == null)
        return NotFound();
      _apiKeyRepository.DeleteApiKey(apiKey);
      await _apiKeyRepository.SaveChangesAsync();
      return NoContent();
    }

    [HttpGet("secret/apikeys/{id}/secret")]
    public async Task<ActionResult<ApiKeySecretDto>> GetApiKeySecret(int id)
    {
      var apiKey = await _apiKeyRepository.GetApiKeyAsync(id);
      if (apiKey == null)
        return NotFound();
      return Ok(_mapper.Map<ApiKeySecretDto>(apiKey));
    }

    [HttpPut("secret/apikeys/{id}/secret")]
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
}