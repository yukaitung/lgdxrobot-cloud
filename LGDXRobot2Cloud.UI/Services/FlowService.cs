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

public interface IFlowService
{
  Task<ApiResponse<(IEnumerable<FlowListDto>?, PaginationHelper?)>> GetFlowsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<FlowDto>> GetFlowAsync(int flowId);
  Task<ApiResponse<bool>> AddFlowAsync(FlowCreateDto flowCreateDto);
  Task<ApiResponse<bool>> UpdateFlowAsync(int flowId, FlowUpdateDto flowUpdateDto);
  Task<ApiResponse<bool>> DeleteFlowAsync(int flowId);
  Task<ApiResponse<string>> SearchFlowsAsync(string name);
}

public sealed class FlowService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient,
    ITokenService tokenService
  ) : BaseService(authenticationStateProvider, httpClient, tokenService), IFlowService
{
  public async Task<ApiResponse<(IEnumerable<FlowListDto>?, PaginationHelper?)>> GetFlowsAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      var url = name != null ? $"Automation/Flows?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"Automation/Flows?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var flows = await JsonSerializer.DeserializeAsync<IEnumerable<FlowListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<FlowListDto>?, PaginationHelper?)> {
          Data = (flows, PaginationHelper),
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

  public async Task<ApiResponse<FlowDto>> GetFlowAsync(int flowId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Automation/Flows/{flowId}");
      if (response.IsSuccessStatusCode)
      {
        var flow = await JsonSerializer.DeserializeAsync<FlowDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<FlowDto> {
          Data = flow,
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

  public async Task<ApiResponse<bool>> AddFlowAsync(FlowCreateDto flowCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(flowCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Automation/Flows", content);
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
  
  public async Task<ApiResponse<bool>> UpdateFlowAsync(int flowId, FlowUpdateDto flowUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(flowUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Automation/Flows/{flowId}", content);
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

  public async Task<ApiResponse<bool>> DeleteFlowAsync(int flowId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Automation/Flows/{flowId}");
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

  public async Task<ApiResponse<string>> SearchFlowsAsync(string name)
  {
    try
    {
      var url = $"Automation/Flows/Search?name={name}";
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
