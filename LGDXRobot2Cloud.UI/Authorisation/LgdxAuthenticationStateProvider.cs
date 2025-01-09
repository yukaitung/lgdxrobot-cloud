using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace LGDXRobot2Cloud.UI.Authorisation;

internal sealed class LgdxAuthenticationStateProvider(
    ILoggerFactory loggerFactory, 
    IRefreshTokenService refreshTokenService,
    ITokenService tokenService, 
    NavigationManager navigationManager
  ) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
  private readonly ITokenService _tokenService = tokenService;
  private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
  private readonly NavigationManager _navigationManager = navigationManager;

  protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
  {
    if (authenticationState.User.Identity?.IsAuthenticated == true)
    {
      var user = authenticationState.User;
      if (!_tokenService.IsLoggedIn(user))
      {
        _navigationManager.NavigateTo(AppRoutes.Identity.Login);
      }

      var refreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiresAt(user);
      if (DateTime.UtcNow > refreshTokenExpiresAt)
      {
        _navigationManager.NavigateTo(AppRoutes.Identity.Login);
      }

      var accessTokenExpiresAt = _tokenService.GetAccessTokenExpiresAt(user);
      if (DateTime.UtcNow.AddMinutes(1) > accessTokenExpiresAt)
      {
        var result = await _refreshTokenService.RefreshTokenAsync(user, _tokenService.GetRefreshToken(user));
        if (result.IsSuccess)
        {
          _tokenService.RefreshAccessToken(user, result.Data!.AccessToken, result.Data.RefreshToken);
        }
      }
    }
    return true;
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
  }
}