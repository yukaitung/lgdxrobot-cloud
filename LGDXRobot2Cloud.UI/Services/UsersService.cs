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

public interface IUsersService
{
  Task<ApiResponse<(IEnumerable<LgdxUserListDto>?, PaginationHelper?)>> GetUsersAsync(string? name = null, int pageNum = 1, int pageSize = 10);
  Task<ApiResponse<LgdxUserDto>> GetUserAsync(string userId);
  Task<ApiResponse<bool>> AddUserAsync(LgdxUserCreateAdminDto lgdxUserCreateAdminDto);
  Task<ApiResponse<bool>> UpdateUserAsync(string userId, LgdxUserUpdateAdminDto lgdxUserUpdateAdminDto);
  Task<ApiResponse<bool>> DeleteUserAsync(string userId);
}

public sealed class UsersService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient,
    ITokenService tokenService
  ) : BaseService(authenticationStateProvider, httpClient, tokenService), IUsersService
{
  public async Task<ApiResponse<(IEnumerable<LgdxUserListDto>?, PaginationHelper?)>> GetUsersAsync(string? name = null, int pageNum = 1, int pageSize = 10)
  {
    try
    {
      var url = name != null ? $"Administration/Users?name={name}&pageNumber={pageNum}&pageSize={pageSize}" : $"Administration/Users?pageNumber={pageNum}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var users = JsonSerializer.Deserialize<IEnumerable<LgdxUserListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<LgdxUserListDto>?, PaginationHelper?)> {
          Data = (users, PaginationHelper),
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

  public async Task<ApiResponse<LgdxUserDto>> GetUserAsync(string userId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Administration/Users/{userId}");
      var user = JsonSerializer.Deserialize<LgdxUserDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<LgdxUserDto> {
          Data = user,
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

  public async Task<ApiResponse<bool>> AddUserAsync(LgdxUserCreateAdminDto lgdxUserCreateAdminDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(lgdxUserCreateAdminDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Administration/Users", content);
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

  public async Task<ApiResponse<bool>> UpdateUserAsync(string userId, LgdxUserUpdateAdminDto lgdxUserUpdateAdminDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(lgdxUserUpdateAdminDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync($"Administration/Users/{userId}", content);
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

  public async Task<ApiResponse<bool>> DeleteUserAsync(string userId)
  {
    try
    {
      var response = await _httpClient.DeleteAsync($"Administration/Users/{userId}");
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
}