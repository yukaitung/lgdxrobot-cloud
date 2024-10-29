using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public interface IApiKeyService
{
  Task<(IEnumerable<ApiKey>?, PaginationHelper?)> GetApiKeysAsync(bool isThirdParty, string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiKey?> GetApiKeyAsync(int apiKeyId);
  Task<bool> AddApiKeyAsync(ApiKeyCreateDto apiKey);
  Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateDto apiKey);
  Task<bool> DeleteApiKeyAsync(int apiKeyId);
  Task<ApiKeySecret?> GetApiKeySecretAsync(int apiKeyId);
  Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretDto apiKey);
  Task<string> SearchApiKeysAsync(string name);
}

public sealed class ApiKeyService(
  AuthenticationStateProvider authenticationStateProvider, 
  HttpClient httpClient) : BaseService(authenticationStateProvider, httpClient), IApiKeyService
{
  public async Task<(IEnumerable<ApiKey>?, PaginationHelper?)> GetApiKeysAsync(bool isThirdParty, string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    var url = name != null ? $"setting/apikeys?isThirdParty={isThirdParty}&name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"setting/secret/apikeys?isThirdParty={isThirdParty}&pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var apiKeys = await JsonSerializer.DeserializeAsync<IEnumerable<ApiKey>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (apiKeys, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<ApiKey?> GetApiKeyAsync(int apiKeyId)
  {
    var response = await _httpClient.GetAsync($"setting/apikeys/{apiKeyId}");
    var apiKeys = await JsonSerializer.DeserializeAsync<ApiKey>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return apiKeys;
  }

  public async Task<bool> AddApiKeyAsync(ApiKeyCreateDto apiKey)
  {
    var apiKeyJson = new StringContent(JsonSerializer.Serialize(apiKey), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("setting/apikeys", apiKeyJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateDto apiKey)
  {
    var apiKeyJson = new StringContent(JsonSerializer.Serialize(apiKey), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"setting/apikeys/{apiKeyId}", apiKeyJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteApiKeyAsync(int apiKeyId)
  {
    var response = await _httpClient.DeleteAsync($"setting/apikeys/{apiKeyId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<ApiKeySecret?> GetApiKeySecretAsync(int apiKeyId)
  {
    var response = await _httpClient.GetAsync($"setting/apikeys/{apiKeyId}/secret");
    var apiKeySecret = await JsonSerializer.DeserializeAsync<ApiKeySecret>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return apiKeySecret;
  }

  public async Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretDto apiKey)
  {
    var apiKeySecretJson = new StringContent(JsonSerializer.Serialize(apiKey), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"setting/apikeys/{apiKeyId}/secret", apiKeySecretJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchApiKeysAsync(string name)
  {
    var url = $"setting/apikeys?isThirdParty=true&name={name}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      return await response.Content.ReadAsStringAsync();
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }
}
