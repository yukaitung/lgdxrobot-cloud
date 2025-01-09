using System.Net;
using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.UI.Services;

public interface IApiKeyService
{
  Task<ApiResponse<(IEnumerable<ApiKeyDto>?, PaginationHelper?)>> GetApiKeysAsync(bool isThirdParty, string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<ApiKeyDto>> GetApiKeyAsync(int apiKeyId);
  Task<ApiResponse<bool>> AddApiKeyAsync(ApiKeyCreateDto apiKeyCreateDto);
  Task<ApiResponse<bool>> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateDto apiKeyUpdateDto);
  Task<ApiResponse<bool>> DeleteApiKeyAsync(int apiKeyId);
  Task<ApiResponse<ApiKeySecretDto>> GetApiKeySecretAsync(int apiKeyId);
  Task<ApiResponse<bool>> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretUpdateDto apiKey);
  Task<ApiResponse<string>> SearchApiKeysAsync(string name);
}

public sealed class ApiKeyService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), IApiKeyService
{
  public async Task<ApiResponse<(IEnumerable<ApiKeyDto>?, PaginationHelper?)>> GetApiKeysAsync(bool isThirdParty, string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      var url = name != null ? $"Administration/ApiKeys?isThirdParty={isThirdParty}&name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"Administration/ApiKeys?isThirdParty={isThirdParty}&pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var apiKeys = await JsonSerializer.DeserializeAsync<IEnumerable<ApiKeyDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<ApiKeyDto>?, PaginationHelper?)> {
          Data = (apiKeys, PaginationHelper),
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<ApiKeyDto>> GetApiKeyAsync(int apiKeyId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Administration/ApiKeys/{apiKeyId}");
      if (response.IsSuccessStatusCode)
      {
        var apiKey = await JsonSerializer.DeserializeAsync<ApiKeyDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<ApiKeyDto> {
          Data = apiKey,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<bool>> AddApiKeyAsync(ApiKeyCreateDto apiKeyCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(apiKeyCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Administration/ApiKeys", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = response.IsSuccessStatusCode,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<bool>> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateDto apiKeyUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(apiKeyUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Administration/ApiKeys/{apiKeyId}", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = response.IsSuccessStatusCode,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<bool>> DeleteApiKeyAsync(int apiKeyId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Administration/ApiKeys/{apiKeyId}");
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = response.IsSuccessStatusCode,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<ApiKeySecretDto>> GetApiKeySecretAsync(int apiKeyId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Administration/ApiKeys/{apiKeyId}/Secret");
      if (response.IsSuccessStatusCode)
      {
        var secretDto = await JsonSerializer.DeserializeAsync<ApiKeySecretDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<ApiKeySecretDto> {
          Data = secretDto,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<bool>> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretUpdateDto apiKeySecretUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(apiKeySecretUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Administration/ApiKeys/{apiKeyId}/Secret", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = response.IsSuccessStatusCode,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<string>> SearchApiKeysAsync(string name)
  {
    try
    {
      var url = $"Administration/ApiKeys/Search?name={name}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<string> {
          Data = await response.Content.ReadAsStringAsync(),
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }
}
