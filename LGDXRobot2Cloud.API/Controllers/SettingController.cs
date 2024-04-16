using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class SettingController : ControllerBase
  {
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IMapper _mapper;
    private readonly int maxPageSize = 100;

    public SettingController(IApiKeyRepository apiKeyRepository,
      IMapper mapper)
    {
      _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /*
    ** Api Key
    */
    [HttpGet("secret/apikeys")]
    public async Task<ActionResult<IEnumerable<ApiKeyDto>>> GetApiKeys(string? name, bool isThirdParty = false, int pageNumber = 1, int pageSize = 10)
    {
      pageSize = (pageSize > maxPageSize) ? maxPageSize : pageSize;
      var (apiKeys, paginationMetadata) = await _apiKeyRepository.GetApiKeysAsync(name, isThirdParty, pageNumber, pageSize);
      Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
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
  }
}