using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace LGDXRobot2Cloud.UI.Services;

public class CircuitHandlerService(
    AuthenticationStateProvider authenticationStateProvider
  ) : CircuitHandler
{
  private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
  public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
  {
    return base.OnCircuitOpenedAsync(circuit, cancellationToken);
  }

  public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
  {
    return base.OnCircuitClosedAsync(circuit, cancellationToken);
  }

  public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
  {
    return base.OnConnectionDownAsync(circuit, cancellationToken);
  }

  public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
  {
    return base.OnConnectionUpAsync(circuit, cancellationToken);
  }
}