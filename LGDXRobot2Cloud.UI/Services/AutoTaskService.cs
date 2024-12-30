using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Utilities.Helpers;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Components.Authorization;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.UI.Services;

public interface IAutoTaskService
{
  Task<ApiResponse<(IEnumerable<AutoTaskListDto>?, PaginationHelper?)>> GetAutoTasksAsync(ProgressState? showProgressId = null, bool? showRunningTasks = null, string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<AutoTaskDto>> GetAutoTaskAsync(int autoTaskId);
  Task<ApiResponse<bool>> AddAutoTaskAsync(AutoTaskCreateDto autoTaskCreateDto);
  Task<ApiResponse<bool>> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateDto autoTaskUpdateDto);
  Task<ApiResponse<bool>> DeleteAutoTaskAsync(int autoTaskId);
  Task<ApiResponse<bool>> AbortAutoTaskAsync(int autoTaskId);
}

public sealed class AutoTaskService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), IAutoTaskService
{
  public async Task<ApiResponse<(IEnumerable<AutoTaskListDto>?, PaginationHelper?)>> GetAutoTasksAsync(ProgressState? showProgressId = null, bool? showRunningTasks = null, string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      StringBuilder url = new($"Automation/AutoTasks?pageNumber={pageNumber}&pageSize={pageSize}");
      if (showProgressId != null)
        url.Append($"&showProgressId={(int)showProgressId}");
      if (showRunningTasks == true)
        url.Append("&showRunningTasks=true");
      var response = await _httpClient.GetAsync(url.ToString());
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var autoTasks = await JsonSerializer.DeserializeAsync<IEnumerable<AutoTaskListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<AutoTaskListDto>?, PaginationHelper?)> {
          Data = (autoTasks, PaginationHelper),
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
  
  public async Task<ApiResponse<AutoTaskDto>> GetAutoTaskAsync(int autoTaskId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Automation/AutoTasks/{autoTaskId}");
      if (response.IsSuccessStatusCode)
      {
        var autoTask = await JsonSerializer.DeserializeAsync<AutoTaskDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<AutoTaskDto> {
          Data = autoTask,
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

  public async Task<ApiResponse<bool>>  AddAutoTaskAsync(AutoTaskCreateDto autoTaskCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(autoTaskCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Automation/AutoTasks", content);
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

  public async Task<ApiResponse<bool>> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateDto autoTaskUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(autoTaskId), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Automation/AutoTasks/{autoTaskId}", content);
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

  public async Task<ApiResponse<bool>> DeleteAutoTaskAsync(int autoTaskId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Automation/AutoTasks/{autoTaskId}");
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

  public async Task<ApiResponse<bool>>  AbortAutoTaskAsync(int autoTaskId)
  {
    try
    {
      var response = await _httpClient.PostAsync($"Automation/AutoTasks/{autoTaskId}/Abort", null);
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
}
