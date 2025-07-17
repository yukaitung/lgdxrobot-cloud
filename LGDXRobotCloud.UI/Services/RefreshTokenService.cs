using System.Security.Claims;
using System.Text;
using System.Text.Json;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Helpers;

namespace LGDXRobotCloud.UI.Services;

public interface IRefreshTokenService
{
  Task<RefreshTokenResponseDto?> RefreshTokenAsync(ClaimsPrincipal user, string refreshToken);
}

public sealed class RefreshTokenService(HttpClient httpClient) : IRefreshTokenService
{
  private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
  private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

  public async Task<RefreshTokenResponseDto?> RefreshTokenAsync(ClaimsPrincipal user, string refreshToken)
  {
    RefreshTokenRequestDto refreshTokenRequestDto = new() {
      RefreshToken = refreshToken
    };
    var content = new StringContent(JsonSerializer.Serialize(refreshTokenRequestDto), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("Identity/Auth/Refresh", content);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<RefreshTokenResponseDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
    {
      // No expection
      return null;
    }
  }
}