using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobotCloud.UI.Components.Pages.Home;

public sealed partial class Tasks : ComponentBase
{
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
  
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  public AutoTaskStatisticsDto? AutoTaskStatisticsDto { get; set; }
  
  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    var realmId = settings.CurrentRealmId;
    AutoTaskStatisticsDto = await LgdxApiClient.Automation.AutoTasks.Statistics[realmId].GetAsync();
    await base.OnInitializedAsync();
  }
}