using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions.Authentication;

namespace LGDXRobotCloud.UI.Authorisation;

public class LgdxAccessTokenProvider(
    AuthenticationStateProvider authenticationStateProvider,
    ITokenService tokenService
  ) : IAccessTokenProvider
{
  private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
  private readonly ITokenService _tokenService = tokenService;

  public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
  {
    var user = _authenticationStateProvider.GetAuthenticationStateAsync().Result.User;

    // Logout if the access token is expired
    var accessTokenExpiresAt = _tokenService.GetAccessTokenExpiresAt(user);
    if (DateTime.UtcNow >= accessTokenExpiresAt)
    {
      _tokenService.Logout(user);
    }

    return Task.FromResult(_tokenService.GetAccessToken(user));
  }

  public AllowedHostsValidator AllowedHostsValidator { get; } = new AllowedHostsValidator();
}