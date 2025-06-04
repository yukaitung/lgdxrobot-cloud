using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Navigation;

public record WaypointTrafficDisplay
{
  public int WaypointFromId { get; set; }
  public int WaypointToId { get; set; }
  public bool IsBothWaysTraffic { get; set; }
}

public sealed class MapEditorViewModel : FormViewModel
{
  public List<WaypointListDto> Waypoints { get; set; } = [];

  public List<WaypointLinkDto> WaypointTraffics { get; set; } = [];

  public List<WaypointTrafficDisplay> WaypointTrafficsDisplay { get; set; } = [];
}

public static class MapEditorViewModelExtensions
{
  public static void FromDto(this MapEditorViewModel mapEditorViewModel, MapEditorDto mapEditorDto)
  {
    // Waypoints
    mapEditorViewModel.Waypoints = mapEditorDto.Waypoints ?? [];

    // Waypoint Traffics
    mapEditorViewModel.WaypointTraffics = mapEditorDto.WaypointLinks ?? [];

    // Key: (WaypointFromId, WaypointToId), Value: IsBothWaysTraffic
    Dictionary<(int, int), bool> waypointTrafficsDisplayTemp = [];
    foreach (var link in mapEditorDto.WaypointLinks!)
    {
      // Display
      // If other way around is exists, then it is both ways traffic
      if (waypointTrafficsDisplayTemp.ContainsKey(((int)link.WaypointToId!, (int)link.WaypointFromId!)))
      {
        waypointTrafficsDisplayTemp[((int)link.WaypointToId!, (int)link.WaypointFromId!)] = true;
      }
      else
      {
        waypointTrafficsDisplayTemp.Add(((int)link.WaypointFromId!, (int)link.WaypointToId!), false);
      }
    }
    // waypointLinksDisplayTemp to waypointLinksDisplay
    mapEditorViewModel.WaypointTrafficsDisplay = waypointTrafficsDisplayTemp.Select(x => new WaypointTrafficDisplay
    {
      WaypointFromId = x.Key.Item1,
      WaypointToId = x.Key.Item2,
      IsBothWaysTraffic = x.Value
    }).ToList();
  }
}