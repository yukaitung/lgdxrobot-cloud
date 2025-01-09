using LGDXRobot2Cloud.UI.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace LGDXRobot2Cloud.UI.Services;

public class CircuitHandlerService(
    AuthenticationStateProvider authenticationStateProvider,
    ITokenService tokenService,
    NavigationManager navigationManager
  ) : CircuitHandler
{
  private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
  private readonly ITokenService _tokenService = tokenService;
  private readonly NavigationManager _navigationManager = navigationManager;

  private void RevalidateLogin()
  {
    var user = _authenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    if (!_tokenService.IsLoggedIn(user))
    {
      _navigationManager.NavigateTo(AppRoutes.Identity.Login);
    }
    if (DateTime.UtcNow > _tokenService.GetRefreshTokenExpiresAt(user))
    {
      _tokenService.Logout(user);
      _navigationManager.NavigateTo(AppRoutes.Identity.Login);
    }
  }

  public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
  {
    RevalidateLogin();
    return base.OnCircuitOpenedAsync(circuit, cancellationToken);
  }
}