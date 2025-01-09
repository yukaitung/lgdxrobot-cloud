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

public interface IProgressService
{
  Task<ApiResponse<(IEnumerable<ProgressDto>?, PaginationHelper?)>> GetProgressesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<ProgressDto>> GetProgressAsync(int progressId);
  Task<ApiResponse<bool>> AddProgressAsync(ProgressCreateDto progressCreateDto);
  Task<ApiResponse<bool>> UpdateProgressAsync(int progressId, ProgressUpdateDto progressUpdateDto);
  Task<ApiResponse<bool>> DeleteProgressAsync(int progressId);
  Task<ApiResponse<string>> SearchProgressesAsync(string name);
}

public sealed class ProgressService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient,
    ITokenService tokenService
  ) : BaseService(authenticationStateProvider, httpClient, tokenService), IProgressService
{
  public async Task<ApiResponse<(IEnumerable<ProgressDto>?, PaginationHelper?)>> GetProgressesAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      var url = name != null ? $"Automation/Progresses?name={name}&pageNumber={pageNumber}&pageSize={pageSize}&hideSystem=true" : $"Automation/Progresses?pageNumber={pageNumber}&pageSize={pageSize}&hideSystem=true";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var progresses = await JsonSerializer.DeserializeAsync<IEnumerable<ProgressDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<ProgressDto>?, PaginationHelper?)> {
          Data = (progresses, PaginationHelper),
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

  public async Task<ApiResponse<ProgressDto>> GetProgressAsync(int progressId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Automation/Progresses/{progressId}");
      if (response.IsSuccessStatusCode)
      {
        var progress = await JsonSerializer.DeserializeAsync<ProgressDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<ProgressDto> {
          Data = progress,
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

  public async Task<ApiResponse<bool>> AddProgressAsync(ProgressCreateDto progressCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(progressCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Automation/Progresses", content);
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

  public async Task<ApiResponse<bool>> UpdateProgressAsync(int progressId, ProgressUpdateDto progressUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(progressUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Automation/Progresses/{progressId}", content);
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

  public async Task<ApiResponse<bool>> DeleteProgressAsync(int progressId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Automation/Progresses/{progressId}");
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

  public async Task<ApiResponse<string>> SearchProgressesAsync(string name)
  {
    try
    {
      var url = $"Automation/Progresses/Search?name={name}";
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