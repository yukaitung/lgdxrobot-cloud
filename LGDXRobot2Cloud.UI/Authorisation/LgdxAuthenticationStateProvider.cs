using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace LGDXRobot2Cloud.UI.Authorisation;

internal sealed class LgdxAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
  private Task<AuthenticationState>? authenticationStateTask;

  protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

  public LgdxAuthenticationStateProvider(ILoggerFactory loggerFactory) : base(loggerFactory)
  {
    AuthenticationStateChanged += OnAuthenticationStateChanged;
  }

  protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
  {
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