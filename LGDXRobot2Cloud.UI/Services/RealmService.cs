using System.Net;
using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRealmService
{
  Task<ApiResponse<(IEnumerable<RealmListDto>?, PaginationHelper?)>> GetRealmsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<RealmDto>> GetRealmAsync(int realmId);
  Task<ApiResponse<bool>> AddRealmAsync(RealmCreateDto realmCreateDto);
  Task<ApiResponse<bool>> UpdateRealmAsync(int realmId, RealmUpdateDto realmUpdateDto);
  Task<ApiResponse<bool>> DeleteRealmAsync(int realmId);

  Task<ApiResponse<string>> SearchRealmsAsync(string name);
  Task<ApiResponse<RealmDto>> GetDefaultRealmAsync();
}

public sealed class RealmService (
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient,
    ITokenService tokenService,
    IMemoryCache memoryCache
  ) : BaseService(authenticationStateProvider, httpClient, tokenService), IRealmService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  public async Task<ApiResponse<(IEnumerable<RealmListDto>?, PaginationHelper?)>> GetRealmsAsync(string? name, int pageNumber, int pageSize)
  {
    try
    {
      var url = name != null ? $"/Navigation/Realms?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"/Navigation/Realms?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var realms = await JsonSerializer.DeserializeAsync<IEnumerable<RealmListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<RealmListDto>?, PaginationHelper?)> {
          Data = (realms, PaginationHelper),
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

  public async Task<ApiResponse<RealmDto>> GetRealmAsync(int realmId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"/Navigation/Realms/{realmId}");
      if (response.IsSuccessStatusCode)
      {
        var realm = await JsonSerializer.DeserializeAsync<RealmDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<RealmDto> {
          Data = realm,
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

  public async Task<ApiResponse<bool>> AddRealmAsync(RealmCreateDto realmCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(realmCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("/Navigation/Realms", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = true,
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

  public async Task<ApiResponse<bool>> UpdateRealmAsync(int realmId, RealmUpdateDto realmUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(realmUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"/Navigation/Realms/{realmId}", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = true,
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

  public async Task<ApiResponse<bool>> DeleteRealmAsync(int realmId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"/Navigation/Realms/{realmId}");
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = true,
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

  public async Task<ApiResponse<string>> SearchRealmsAsync(string name)
  {
    try
    {
      var url = $"/Navigation/Realms?name={name}";
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

  public async Task<ApiResponse<RealmDto>> GetDefaultRealmAsync()
  {
    if (_memoryCache.TryGetValue($"RealmService_GetDefaultRealm", out RealmDto? cachedMap))
    {
      return new ApiResponse<RealmDto> {
        Data = cachedMap,
        IsSuccess = true
      };
    }
    try
    {
      var response = await _httpClient.GetAsync("/Navigation/Realms/Default");
      if (response.IsSuccessStatusCode)
      {
        var map = await JsonSerializer.DeserializeAsync<RealmDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        _memoryCache.Set($"RealmService_GetDefaultRealm", map);
        return new ApiResponse<RealmDto> {
          Data = map,
          IsSuccess = true
        };
      }
      else
      {
        // Default Realm if not found
        RealmDto map = new RealmDto {
          Id = 0,
          Name = "Default",
          Description = "Default Realm",
          Image = "",
          Resolution = 0.0,
          OriginX = 0.0,
          OriginY = 0.0,
          OriginRotation = 0.0
        };
        _memoryCache.Set($"RealmService_GetDefaultRealm", map);
        return new ApiResponse<RealmDto> {
          Data = map,
          IsSuccess = true
        };
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }
}