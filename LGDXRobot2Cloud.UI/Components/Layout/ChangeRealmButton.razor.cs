using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Components.Layout;

public sealed partial class ChangeRealmButton : ComponentBase
{
  [Inject]
  public required IRealmService RealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  public string RealmName { get; set; } = string.Empty;

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    var response = await RealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);
    var realm = response.Data;
    RealmName = realm!.Name;
    await base.OnInitializedAsync();
  }
}