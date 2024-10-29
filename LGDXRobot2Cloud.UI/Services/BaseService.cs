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
      _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {user.FindFirst("access_token")?.Value.ToString()}");
    }
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }
}