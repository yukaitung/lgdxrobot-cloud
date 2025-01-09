using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace LGDXRobot2Cloud.UI.Authorisation;

internal sealed class LgdxAuthenticationStateProvider(
    ILoggerFactory loggerFactory, 
    NavigationManager navigationManager
  ) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
  private readonly NavigationManager _navigationManager = navigationManager;

  protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
  {
    if (authenticationState.User.Identity?.IsAuthenticated == true)
    {

    }
    return true;
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
  }
}