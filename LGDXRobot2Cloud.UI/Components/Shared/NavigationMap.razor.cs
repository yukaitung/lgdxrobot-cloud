using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Shared;

public sealed partial class NavigationMap : ComponentBase
{
  [Inject]
  public required IRealmService RealmService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IMemoryCache MemoryCache { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  private RealmDto Map { get; set; } = null!;
  private Dictionary<Guid, RobotDataContract> RobotsData { get; set; } = [];

  protected override async Task OnInitializedAsync() 
  {
    var onlineRobots = RobotDataService.GetOnlineRobots();
    foreach (var robotId in onlineRobots)
    {
      var robotData = RobotDataService.GetRobotData(robotId);
      if (robotData != null)
      {
        RobotsData.Add(robotId, robotData);
      }
    }
    var response = await RealmService.GetDefaultRealmAsync();
    var map = response.Data;
    if (map != null)
    {
      Map = map;
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