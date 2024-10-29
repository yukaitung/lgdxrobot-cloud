using LGDXRobot2Cloud.Data.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface IUserService
{
  Task<bool> LoginAsync(HttpContext context, LoginRequest request);
}

public class UserService : IUserService
{
  private readonly HttpClient _httpClient;
  private readonly JsonSerializerOptions _jsonSerializerOptions;

  public UserService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<bool> LoginAsync(HttpContext context, LoginRequest request)
  {
    var json = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("/Identity/User/login", json);
    if (response.IsSuccessStatusCode)
    {
      var loginResponse = await JsonSerializer.DeserializeAsync<LoginResponse>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse!.AccessToken);
      var identity = new ClaimsIdentity([], CookieAuthenticationDefaults.AuthenticationScheme);
      identity.AddClaims(accessToken.Claims);
      identity.AddClaim(new Claim("access_token", loginResponse!.AccessToken));
      var user = new ClaimsPrincipal(identity);
      var authProperties = new AuthenticationProperties{};
      await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user, authProperties);
      return true;
    }
    else
    {
      return false;
    }
  }
}