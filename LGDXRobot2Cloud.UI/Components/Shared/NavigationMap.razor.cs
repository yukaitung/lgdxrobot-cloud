using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Shared;

public sealed partial class NavigationMap : ComponentBase
{
  [Inject]
  public required IMapsService MapsService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  Map Map { get; set; } = null!;

  protected override async Task OnInitializedAsync() {
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
    }
    await base.OnAfterRenderAsync(firstRender);
  }
}