using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public interface ITriggerRetryService
{
  Task<ApiResponse<(IEnumerable<TriggerRetryListDto>?, PaginationHelper?)>> GetTriggerRetriesAsync(int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<TriggerRetryDto>> GetTriggerRetryAsync(int triggerRetryId);
  Task<ApiResponse<bool>> DeleteTriggerRetryAsync(int triggerRetryId);
}

public sealed class TriggerRetryService (
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), ITriggerRetryService
{
  public async Task<ApiResponse<(IEnumerable<TriggerRetryListDto>?, PaginationHelper?)>> GetTriggerRetriesAsync(int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      var url = $"Automation/TriggerRetries?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var triggerRetries = await JsonSerializer.DeserializeAsync<IEnumerable<TriggerRetryListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<TriggerRetryListDto>?, PaginationHelper?)> {
          Data = (triggerRetries, PaginationHelper),
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

  public async Task<ApiResponse<TriggerRetryDto>> GetTriggerRetryAsync(int triggerRetryId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Automation/TriggerRetries/{triggerRetryId}");
      if (response.IsSuccessStatusCode)
      {
        var triggerRetry = await JsonSerializer.DeserializeAsync<TriggerRetryDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<TriggerRetryDto> {
          Data = triggerRetry,
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

  public async Task<ApiResponse<bool>> DeleteTriggerRetryAsync(int triggerRetryId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Automation/TriggerRetries/{triggerRetryId}");
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
}