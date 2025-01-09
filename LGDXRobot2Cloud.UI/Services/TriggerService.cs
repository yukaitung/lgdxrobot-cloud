using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface ITriggerService
{
  Task<ApiResponse<(IEnumerable<TriggerListDto>?, PaginationHelper?)>> GetTriggersAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<TriggerDto>> GetTriggerAsync(int triggerId);
  Task<ApiResponse<bool>> AddTriggerAsync(TriggerCreateDto triggerCreateDto);
  Task<ApiResponse<bool>> UpdateTriggerAsync(int triggerId, TriggerUpdateDto triggerUpdateDto);
  Task<ApiResponse<bool>> DeleteTriggerAsync(int triggerId);
  Task<ApiResponse<string>>SearchTriggersAsync(string name);
}

public sealed class TriggerService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), ITriggerService
{
  public async Task<ApiResponse<(IEnumerable<TriggerListDto>?, PaginationHelper?)>> GetTriggersAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      var url = name != null ? $"Automation/Triggers?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"Navigation/Waypoints?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var triggers = await JsonSerializer.DeserializeAsync<IEnumerable<TriggerListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<TriggerListDto>?, PaginationHelper?)> {
          Data = (triggers, PaginationHelper),
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

  public async Task<ApiResponse<TriggerDto>> GetTriggerAsync(int triggerId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Automation/Triggers/{triggerId}");
      if (response.IsSuccessStatusCode)
      {
        var trigger = await JsonSerializer.DeserializeAsync<TriggerDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<TriggerDto> {
          Data = trigger,
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

  public async Task<ApiResponse<bool>> AddTriggerAsync(TriggerCreateDto triggerCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(triggerCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Automation/Triggers", content);
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

  public async Task<ApiResponse<bool>> UpdateTriggerAsync(int triggerId, TriggerUpdateDto triggerUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(triggerUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Automation/Triggers/{triggerId}", content);
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

  public async Task<ApiResponse<bool>> DeleteTriggerAsync(int triggerId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Automation/Triggers/{triggerId}");
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

  public async Task<ApiResponse<string>>SearchTriggersAsync(string name)
  {
    try
    {
      var url = $"Automation/Triggers/Search?name={name}";
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