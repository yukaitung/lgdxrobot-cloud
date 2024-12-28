using System.Net;
using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.UI.Services;

public interface IUserService
{
  Task<ApiResponse<LgdxUserDto>> GetUserAsync();
  Task<ApiResponse<bool>> UpdateUserAsync(LgdxUserUpdateDto lgdxUserUpdateDto);
  Task<ApiResponse<bool>> UpdatePasswordAsync(UpdatePasswordRequestDto updatePasswordRequestDto);
}

public sealed class UserService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), IUserService
{
  public async Task<ApiResponse<LgdxUserDto>> GetUserAsync()
  {
    try
    {
      var response = await _httpClient.GetAsync("Identity/User");
      if (response.IsSuccessStatusCode)
      {
        var lgdxUserDto = JsonSerializer.Deserialize<LgdxUserDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<LgdxUserDto> {
          Data = lgdxUserDto,
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

  public async Task<ApiResponse<bool>> UpdateUserAsync(LgdxUserUpdateDto user)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
      var response = await _httpClient.PutAsync("Identity/User", content);
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

  public async Task<ApiResponse<bool>> UpdatePasswordAsync(UpdatePasswordRequestDto updatePasswordRequestDto)
  {
    try
    {
      var userJson = new StringContent(JsonSerializer.Serialize(updatePasswordRequestDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Identity/User/Password", userJson);
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