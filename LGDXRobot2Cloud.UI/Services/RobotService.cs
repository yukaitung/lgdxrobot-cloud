using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotService
{
  Task<ApiResponse<(IEnumerable<RobotListDto>?, PaginationHelper?)>> GetRobotsAsync(string? name = null, int pageNumber = 1, int pageSize = 16);
  Task<ApiResponse<RobotDto>> GetRobotAsync(string robotId);
  Task<ApiResponse<RobotCertificateIssueDto>> AddRobotAsync(RobotCreateDto robotCreateDto);
  Task<ApiResponse<bool>> SetSoftwareEmergencyStopAsync(string robotId, bool enable);
  Task<ApiResponse<bool>> SetPauseTaskAssigementAsync(string robotId, bool enable);
  Task<ApiResponse<bool>> UpdateRobotAsync(string robotId, RobotUpdateDto robotUpdateDto);
  Task<ApiResponse<bool>> UpdateRobotChassisInfoAsync(string robotId, RobotChassisInfoUpdateDto robotChassisInfoUpdateDto);
  Task<ApiResponse<bool>> DeleteRobotAsync(string robotId);
  Task<ApiResponse<string>> SearchRobotsAsync(string name);
}

public sealed class RobotService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), IRobotService
{
  public async Task<ApiResponse<(IEnumerable<RobotListDto>?, PaginationHelper?)>> GetRobotsAsync(string? name, int pageNumber, int pageSize)
  {
    try
    {
      var url = name != null ? $"Navigation/Robots?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"Navigation/Robots?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var robots = await JsonSerializer.DeserializeAsync<IEnumerable<RobotListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<RobotListDto>?, PaginationHelper?)> {
          Data = (robots, PaginationHelper),
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"The API service returns status code {response.StatusCode}.");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<RobotDto>> GetRobotAsync(string robotId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Navigation/Robots/{robotId}");
      if (response.IsSuccessStatusCode)
      {
        var robot = await JsonSerializer.DeserializeAsync<RobotDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<RobotDto> {
          Data = robot,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"The API service returns status code {response.StatusCode}.");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }
  
  public async Task<ApiResponse<RobotCertificateIssueDto>> AddRobotAsync(RobotCreateDto robotCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(robotCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Navigation/Robots", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<RobotCertificateIssueDto> {
          Data = await JsonSerializer.DeserializeAsync<RobotCertificateIssueDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions),
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<RobotCertificateIssueDto> {
          Errors = validationProblemDetails?.Errors,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"The API service returns status code {response.StatusCode}.");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }
  public async Task<ApiResponse<bool>> SetSoftwareEmergencyStopAsync(string robotId, bool enable)
  {
    try
    {
      EnableDto data = new() { Enable = enable };
      var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
      var response = await _httpClient.PatchAsync($"Navigation/Robots/{robotId}/EmergencyStop", content);
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

  public async Task<ApiResponse<bool>> SetPauseTaskAssigementAsync(string robotId, bool enable)
  {
    try
    {
      EnableDto data = new() { Enable = enable };
      var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
      var response = await _httpClient.PatchAsync($"Navigation/Robots/{robotId}/PauseTaskAssigement", content);
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

  public async Task<ApiResponse<bool>> UpdateRobotAsync(string robotId, RobotUpdateDto robotUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(robotUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Navigation/Robots/{robotId}", content);
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

  public async Task<ApiResponse<bool>> UpdateRobotChassisInfoAsync(string robotId, RobotChassisInfoUpdateDto robotChassisInfoUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(robotChassisInfoUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Navigation/Robots/{robotId}/Chassis", content);
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

  public async Task<ApiResponse<bool>> DeleteRobotAsync(string robotId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Navigation/Robots/{robotId}");
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

  public async Task<ApiResponse<string>> SearchRobotsAsync(string name)
  {
    try
    {
      var url = $"Navigation/Robots?name={name}";
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