using LGDXRobotCloud.UI.Services;
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
    return Task.FromResult(_tokenService.GetAccessToken(user));
  }

  public AllowedHostsValidator AllowedHostsValidator { get; } = new AllowedHostsValidator();
}