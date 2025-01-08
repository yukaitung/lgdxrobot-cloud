using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace LGDXRobot2Cloud.UI.Services;

public class CircuitHandlerService(
    AuthenticationStateProvider authenticationStateProvider,
    ITokenService tokenService
  ) : CircuitHandler
{
  private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
  private readonly ITokenService _tokenService = tokenService;

  public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
  {
    var user = _authenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    _tokenService.Logout(user);
    return base.OnCircuitClosedAsync(circuit, cancellationToken);
  }
}