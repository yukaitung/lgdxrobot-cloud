using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.UI.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public interface ITokenRefreshService
{
  Task<ApiResponse<bool>> RefreshTokenAsync();
}

public class TokenRefreshService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient,
    ITokenService tokenService
  ) : BaseService(authenticationStateProvider, httpClient, tokenService), ITokenRefreshService
{
  public async Task<ApiResponse<bool>> RefreshTokenAsync()
  {
    try
    {
      var user = _authenticationStateProvider.GetAuthenticationStateAsync().Result.User;
      RefreshTokenRequestDto refreshTokenRequestDto = new() {
        RefreshToken = _tokenService.GetRefreshToken(user)
      };
      var content = new StringContent(JsonSerializer.Serialize(refreshTokenRequestDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Identity/Auth/Refresh", content);
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