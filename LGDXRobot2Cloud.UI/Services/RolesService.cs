using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRolesService
{
  Task<ApiResponse<(IEnumerable<LgdxRoleDto>?, PaginationHelper?)>> GetRolesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<LgdxRoleDto>> GetRoleAsync(string id);
  Task<ApiResponse<bool>> AddRoleAsync(LgdxRoleCreateDto lgdxRoleCreateDto);
  Task<ApiResponse<bool>> UpdateRoleAsync(string id, LgdxRoleUpdateDto lgdxRoleUpdateDto);
  Task<ApiResponse<bool>> DeleteRoleAsync(string id);
  Task<ApiResponse<string>> SearchRolesAsync(string name);
}

public sealed class RolesService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), IRolesService
{
  public async Task<ApiResponse<(IEnumerable<LgdxRoleDto>?, PaginationHelper?)>> GetRolesAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      var url = name != null ? $"Administration/Roles?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"Administration/Roles?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var roles = await JsonSerializer.DeserializeAsync<IEnumerable<LgdxRoleDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<LgdxRoleDto>?, PaginationHelper?)> {
          Data = (roles, PaginationHelper),
          IsSuccess = true
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

  public async Task<ApiResponse<LgdxRoleDto>> GetRoleAsync(string id)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Administration/Roles/{id}");
      if (response.IsSuccessStatusCode)
      {
        var role = await JsonSerializer.DeserializeAsync<LgdxRoleDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<LgdxRoleDto> {
          Data = role,
          IsSuccess = true
        };
      }
      else if (response.StatusCode == HttpStatusCode.NotFound)
      {
        return new ApiResponse<LgdxRoleDto> {
          Errors = new Dictionary<string, string[]> {
            { "Api", ["The role does not exist."] }
          },
          IsSuccess = false
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

  public async Task<ApiResponse<bool>> AddRoleAsync(LgdxRoleCreateDto lgdxRoleCreateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(lgdxRoleCreateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Administration/Roles", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = true,
          IsSuccess = true
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
          IsSuccess = false
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

  public async Task<ApiResponse<bool>> UpdateRoleAsync(string id, LgdxRoleUpdateDto lgdxRoleUpdateDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(lgdxRoleUpdateDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Administration/Roles/{id}", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = true,
          IsSuccess = true
        };
      }
      else if (response.StatusCode == HttpStatusCode.NotFound)
      {
        return new ApiResponse<bool> {
          Errors = new Dictionary<string, string[]> {
            { "Api", ["The role does not exist."] }
          },
          IsSuccess = false
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
          IsSuccess = false
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

  public async Task<ApiResponse<bool>> DeleteRoleAsync(string id)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Administration/Roles/{id}");
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = true,
          IsSuccess = true
        };
      }
      else if (response.StatusCode == HttpStatusCode.NotFound)
      {
        return new ApiResponse<bool> {
          Errors = new Dictionary<string, string[]> {
            { "Api", ["The role does not exist."] }
          },
          IsSuccess = false
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
          IsSuccess = false
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

  public async Task<ApiResponse<string>> SearchRolesAsync(string name)
  {
    try
    {
      var url = $"Administration/Roles?name={name}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<string> {
          Data = await response.Content.ReadAsStringAsync(),
          IsSuccess = true
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