using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Shared;

public sealed partial class NavigationMap
{
  [Inject]
  public required IMapsService MapsService { get; set; }

  Map Map { get; set; } = null!;

  protected override async Task OnInitializedAsync() {
    var map = await MapsService.GetDefaultMapAsync();
    if (map != null)
    {
      Map = map;
    }
  }
}