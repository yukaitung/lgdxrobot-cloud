using System.Security.Claims;
using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRefreshTokenService
{
  Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(ClaimsPrincipal user, string refreshToken);
}

public sealed class RefreshTokenService : IRefreshTokenService
{
  private readonly HttpClient _httpClient;
  private readonly JsonSerializerOptions _jsonSerializerOptions;

  public RefreshTokenService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(ClaimsPrincipal user, string refreshToken)
  {
    try
    {
      RefreshTokenRequestDto refreshTokenRequestDto = new() {
        RefreshToken = refreshToken
      };
      var content = new StringContent(JsonSerializer.Serialize(refreshTokenRequestDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("Identity/Auth/Refresh", content);
      if (response.IsSuccessStatusCode)
      {
        var refreshTokenResponseDto = await JsonSerializer.DeserializeAsync<RefreshTokenResponseDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<RefreshTokenResponseDto> {
          Data = refreshTokenResponseDto,
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