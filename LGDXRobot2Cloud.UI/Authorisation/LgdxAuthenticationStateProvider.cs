using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace LGDXRobot2Cloud.UI.Authorisation;

internal sealed class LgdxAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
  private Task<AuthenticationState>? authenticationStateTask;
  private readonly ITokenService _tokenService;
  private readonly NavigationManager _navigationManager;

  protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

  public LgdxAuthenticationStateProvider(ILoggerFactory loggerFactory, ITokenService tokenService, NavigationManager navigationManager) : base(loggerFactory)
  {
    AuthenticationStateChanged += OnAuthenticationStateChanged;
    _tokenService = tokenService;
    _navigationManager = navigationManager;
  }

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
      if (DateTime.Now > refreshTokenExpiresAt)
      {
        _navigationManager.NavigateTo(AppRoutes.Identity.Login);
      }

      var accessTokenExpiresAt = _tokenService.GetAccessTokenExpiresAt(user);
      if (DateTime.Now.AddMinutes(1) > accessTokenExpiresAt)
      {
        //await _tokenRefreshService.RefreshTokenAsync();
      }
    }
    return true;
  }

  private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
  {
    authenticationStateTask = task;
  }

  protected override void Dispose(bool disposing)
  {
    AuthenticationStateChanged -= OnAuthenticationStateChanged;
    base.Dispose(disposing);
  }
}