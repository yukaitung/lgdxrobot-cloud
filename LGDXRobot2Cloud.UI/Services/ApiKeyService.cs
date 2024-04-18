using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public class ApiKeyService : IApiKeyService
  {
    public readonly HttpClient _httpClient;
    public readonly JsonSerializerOptions _jsonSerializerOptions;

    public ApiKeyService(HttpClient httpClient)
    {
      _httpClient = httpClient;
      _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true};
    }

    public async Task<(IEnumerable<ApiKey>?, PaginationMetadata?)> GetApiKeysAsync(bool isThirdParty, string? name = null, int pageNumber = 1, int pageSize = 10)
    {
      var url = name != null ? $"setting/secret/apikeys?isThirdParty={isThirdParty}&name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"setting/secret/apikeys?isThirdParty={isThirdParty}&pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var paginationMetadataJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
        var apiKeys = await JsonSerializer.DeserializeAsync<IEnumerable<ApiKey>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return (apiKeys, paginationMetadata);
      }
      else
      {
        throw new Exception($"The API service returns status code {response.StatusCode}.");
      }
    }

    public async Task<ApiKey?> GetApiKeyAsync(int apiKeyId)
    {
      var response = await _httpClient.GetAsync($"setting/secret/apikeys/{apiKeyId}");
      var apiKeys = await JsonSerializer.DeserializeAsync<ApiKey>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return apiKeys;
    }

    public async Task<ApiKey?> AddApiKeyAsync(ApiKeyCreateDto apiKey)
    {
      var apiKeyJson = new StringContent(JsonSerializer.Serialize(apiKey), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("setting/secret/apikeys", apiKeyJson);
      if (response.IsSuccessStatusCode)
      {
        return await JsonSerializer.DeserializeAsync<ApiKey>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      }
      else
        return null;
    }

    public async Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateDto apiKey)
    {
      var apiKeyJson = new StringContent(JsonSerializer.Serialize(apiKey), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"setting/secret/apikeys/{apiKeyId}", apiKeyJson);
      return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteApiKeyAsync(int apiKeyId)
    {
      var response = await _httpClient.DeleteAsync($"setting/secret/apikeys/{apiKeyId}");
      return response.IsSuccessStatusCode;
    }

    public async Task<ApiKeySecretDto?> GetApiKeySecretAsync(int apiKeyId)
    {
      var response = await _httpClient.GetAsync($"setting/secret/apikeys/{apiKeyId}/secret");
      var apiKeySecret = await JsonSerializer.DeserializeAsync<ApiKeySecretDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return apiKeySecret;
    }

    public async Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretDto apiKey)
    {
      var apiKeySecretJson = new StringContent(JsonSerializer.Serialize(apiKey), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"setting/secret/apikeys/{apiKeyId}/secret", apiKeySecretJson);
      return response.IsSuccessStatusCode;
    }
  }
}