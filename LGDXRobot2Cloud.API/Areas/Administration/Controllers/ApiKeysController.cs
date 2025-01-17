using LGDXRobot2Cloud.API.Authorisation;
using LGDXRobot2Cloud.API.Configurations;
using LGDXRobot2Cloud.API.Services.Administration;
using LGDXRobot2Cloud.Data.Models.Business.Administration;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LGDXRobot2Cloud.API.Areas.Administration.Controllers;

[ApiController]
[Area("Administration")]
[Route("[area]/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateLgdxUserAccess]
public sealed class ApiKeysController(
    IApiKeyService apiKeyService,
    IOptionsSnapshot<LgdxRobot2Configuration> lgdxRobot2Configuration
  ) : ControllerBase
{
  private readonly IApiKeyService _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
  private readonly LgdxRobot2Configuration _lgdxRobot2Configuration = lgdxRobot2Configuration.Value ?? throw new ArgumentNullException(nameof(lgdxRobot2Configuration));

  

  [HttpGet("")]
  [ProducesResponseType(typeof(IEnumerable<ApiKeyDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ApiKeyDto>>> GetApiKeys(string? name, bool isThirdParty = false, int pageNumber = 1, int pageSize = 10)
  {
    pageSize = (pageSize > _lgdxRobot2Configuration.ApiMaxPageSize) ? _lgdxRobot2Configuration.ApiMaxPageSize : pageSize;
    var (apiKeys, PaginationHelper) = await _apiKeyService.GetApiKeysAsync(name, isThirdParty, pageNumber, pageSize);
    Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(PaginationHelper));
    return Ok(apiKeys.ToDto());
  }

  [HttpGet("Search")]
  [ProducesResponseType(typeof(IEnumerable<ApiKeySearchDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<ApiKeySearchDto>>> SearchApiKeys(string? name)
  {
    var apikeys = await _apiKeyService.SearchApiKeysAsync(name);
    return Ok(apikeys.ToDto());
  }

  [HttpGet("{id}", Name = "GetApiKey")]
  [ProducesResponseType(typeof(ApiKeyDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ApiKeyDto>> GetApiKey(int id)
  {
    var apiKey = await _apiKeyService.GetApiKeyAsync(id);
    return Ok(apiKey.ToDto());
  }

  [HttpPost("")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  public async Task<ActionResult> CreateApiKey(ApiKeyCreateDto apiKeyCreateDto)
  {
    var apiKey = await _apiKeyService.AddApiKeyAsync(apiKeyCreateDto.ToBusinessModel());
    return CreatedAtRoute(nameof(GetApiKey), new { id = apiKey.Id }, apiKey.ToDto());
  }

  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateApiKey(int id, ApiKeyUpdateDto apiKeyUpdateDto)
  {
    if (!await _apiKeyService.UpdateApiKeyAsync(id, apiKeyUpdateDto.ToBusinessModel()))
    {
      return NotFound();
    }
    return NoContent();
  }

  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> DeleteApiKey(int id)
  {
    if (!await _apiKeyService.DeleteApiKeyAsync(id))
    {
      return NotFound();
    }
    return NoContent();
  }

  [HttpGet("{id}/Secret")]
  [ProducesResponseType(typeof(ApiKeySecretDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ApiKeySecretDto>> GetApiKeySecret(int id)
  {
    var apiKey = await _apiKeyService.GetApiKeyAsync(id);
    return Ok(apiKey.ToDto());
  }

  [HttpPut("{id}/Secret")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult> UpdateApiKeySecret(int id, ApiKeySecretUpdateDto apiKeySecretUpdateDto)
  {
    await _apiKeyService.UpdateApiKeySecretAsync(id, apiKeySecretUpdateDto.ToBusinessModel());
    return NoContent();
  }
}