using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public abstract class BaseService
{
  protected readonly AuthenticationStateProvider _authenticationStateProvider;
  protected readonly HttpClient _httpClient;
  protected readonly JsonSerializerOptions _jsonSerializerOptions;
  protected readonly ITokenService _tokenService;

  public BaseService(AuthenticationStateProvider authenticationStateProvider, HttpClient httpClient, ITokenService tokenService)
  {
    _authenticationStateProvider = authenticationStateProvider;
    _httpClient = httpClient;
    _tokenService = tokenService;
    var user = _authenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    if (user.Identity?.IsAuthenticated == true)
    {
      var token = _tokenService.GetAccessToken(user);
      _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }
}