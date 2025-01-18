using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class RobotInfoForm : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public RobotDetailViewModel? Robot { get; set; }

  public override Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<RobotDetailViewModel?>(nameof(Robot), out var _robot))
    {
      if (_robot != null && _robot.RealmId == null)
      {
        var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
        var settings = TokenService.GetSessionSettings(user);
        _robot.RealmId = settings.CurrentRealmId;
        _robot.RealmName = CachedRealmService.GetRealmName(settings.CurrentRealmId);
      }
    }
    return base.SetParametersAsync(parameters);
  }
}