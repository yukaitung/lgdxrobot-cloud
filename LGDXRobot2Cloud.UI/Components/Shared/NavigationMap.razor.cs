using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Shared;

public sealed partial class NavigationMap : ComponentBase
{
  [Inject]
  public required IMapsService MapsService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IMemoryCache MemoryCache { get; set; }

  private Map Map { get; set; } = null!;
  private Dictionary<Guid, RobotDataContract> RobotsData { get; set; } = null!;

  protected override async Task OnInitializedAsync() 
  {
    RobotsData = MemoryCache.Get<Dictionary<Guid, RobotDataContract>>($"RobotDataConsumer_RobotsData") ?? [];
    var map = await MapsService.GetDefaultMapAsync();
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