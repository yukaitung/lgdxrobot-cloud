using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace LGDXRobotCloud.UI.Authorisation;

internal class LgdxAuthenticationStateProvider(
    ILoggerFactory loggerFactory, 
    IRefreshTokenService refreshTokenService,
    ITokenService tokenService, 
    NavigationManager navigationManager
  ) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
  private readonly ITokenService _tokenService = tokenService;
  private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
  private readonly NavigationManager _navigationManager = navigationManager;

  protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(15);

  protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
  {
    var user = authenticationState.User;
    if (!_tokenService.IsLoggedIn(user))
    {
      _navigationManager.NavigateTo(AppRoutes.Identity.Login + "?ReturnUrl=" + _navigationManager.ToBaseRelativePath(_navigationManager.Uri));
      _navigationManager.Refresh(true);
      return false;
    }

    var refreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiresAt(user);
    if (DateTime.UtcNow.AddMinutes(5) > refreshTokenExpiresAt)
    {
      _tokenService.Logout(user);
      _navigationManager.NavigateTo(AppRoutes.Identity.Login + "?ReturnUrl=" + _navigationManager.ToBaseRelativePath(_navigationManager.Uri));
      _navigationManager.Refresh(true);
      return false;
    }

    var accessTokenExpiresAt = _tokenService.GetAccessTokenExpiresAt(user);
    if (DateTime.UtcNow.AddMinutes(5) >= accessTokenExpiresAt)
    {
      var result = await _refreshTokenService.RefreshTokenAsync(user, _tokenService.GetRefreshToken(user));
      if (result != null)
      {
        _tokenService.RefreshAccessToken(user, result);
      }
    }

    return true;
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
  }
}