using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public abstract class BaseService
{
  protected readonly AuthenticationStateProvider _authenticationStateProvider;
  protected readonly HttpClient _httpClient;
  protected readonly JsonSerializerOptions _jsonSerializerOptions;

  public BaseService(AuthenticationStateProvider authenticationStateProvider, HttpClient httpClient)
  {
    _authenticationStateProvider = authenticationStateProvider;
    _httpClient = httpClient;
    var user = _authenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    if (user.Identity?.IsAuthenticated == true)
    {
      var token = user.Claims.FirstOrDefault(x => x.Type == "access_token")?.Value ?? string.Empty;
      _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }
}