using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Shared;

public sealed partial class NavigationMap : ComponentBase
{
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  private RealmDto Map { get; set; } = null!;
  private Dictionary<Guid, RobotDataContract> RobotsData { get; set; } = [];

  protected override async Task OnInitializedAsync() 
  {
    // Get Realm
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    Map = await CachedRealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);
    var realmId = Map.Id ?? 0;

    // Set Online Robots
    var onlineRobots = RobotDataService.GetOnlineRobots(realmId!);
    foreach (var robotId in onlineRobots)
    {
      var robotData = RobotDataService.GetRobotData(robotId, realmId!);
      if (robotData != null)
      {
        RobotsData.Add(robotId, robotData);
      }
    }
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      await JSRuntime.InvokeVoidAsync("InitNavigationMap");
      foreach (var (robotId, robotData) in RobotsData)
      {
        await JSRuntime.InvokeVoidAsync("AddRobot", robotId, robotData.Position.X, robotData.Position.Y, robotData.Position.Rotation);
      }
    }
    await base.OnAfterRenderAsync(firstRender);
  }
}