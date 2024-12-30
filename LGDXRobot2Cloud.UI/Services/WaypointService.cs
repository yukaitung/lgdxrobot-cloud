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

public interface IWaypointService
{
  Task<ApiResponse<(IEnumerable<WaypointListDto>?, PaginationHelper?)>> GetWaypointsAsync(string? name, int pageNumber, int pageSize);
  Task<ApiResponse<WaypointDto>> GetWaypointAsync(int waypointId);
  Task<ApiResponse<bool>> AddWaypointAsync(WaypointCreateDto waypointCreateDto);
  Task<ApiResponse<bool>> UpdateWaypointAsync(int waypointId, WaypointUpdateDto waypointUpdateDto);
  Task<ApiResponse<bool>> DeleteWaypointAsync(int waypointId);
  Task<ApiResponse<string>> SearchWaypointsAsync(string name);
}

public sealed class WaypointService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), IWaypointService
{
  public async Task<ApiResponse<(IEnumerable<WaypointListDto>?, PaginationHelper?)>> GetWaypointsAsync(string? name, int pageNumber, int pageSize)
  {
    try
    {
      var url = name != null ? $"Navigation/Waypoints?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"Navigation/Waypoints?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var waypoints = await JsonSerializer.DeserializeAsync<IEnumerable<WaypointListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<WaypointListDto>?, PaginationHelper?)> {
          Data = (waypoints, PaginationHelper),
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

  public async Task<ApiResponse<WaypointDto>> GetWaypointAsync(int waypointId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Navigation/Waypoints/{waypointId}");
      if (response.IsSuccessStatusCode)
      {
        var waypoint = await JsonSerializer.DeserializeAsync<WaypointDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<WaypointDto> {
          Data = waypoint,
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

  public async Task<ApiResponse<bool>> AddWaypointAsync(WaypointCreateDto waypointCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(waypointCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Navigation/Waypoints", content);
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

  public async Task<ApiResponse<bool>> UpdateWaypointAsync(int waypointId, WaypointUpdateDto waypointUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(waypointUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Navigation/Waypoints/{waypointId}", content);
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

  public async Task<ApiResponse<bool>> DeleteWaypointAsync(int waypointId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Navigation/Waypoints/{waypointId}");
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

  public async Task<ApiResponse<string>> SearchWaypointsAsync(string name)
  {
    try
    {
      var url = $"Navigation/Waypoints/Search?name={name}";
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