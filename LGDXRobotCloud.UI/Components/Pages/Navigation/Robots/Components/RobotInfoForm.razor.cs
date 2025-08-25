using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Robots.Components;

public partial class RobotInfoForm : ComponentBase
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

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<RobotDetailViewModel?>(nameof(Robot), out var _robot))
    {
      if (_robot != null && _robot.RealmId == null)
      {
        var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
        var settings = TokenService.GetSessionSettings(user);
        _robot.RealmId = settings.CurrentRealmId;
        _robot.RealmName = await CachedRealmService.GetRealmName(settings.CurrentRealmId);
      }
    }
    await base.SetParametersAsync(parameters);
  }
}