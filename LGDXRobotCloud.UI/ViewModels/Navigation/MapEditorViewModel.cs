using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Navigation;

public record WaypointLinkDisplay
{
  public int WaypointFromId { get; set; }
  public int WaypointToId { get; set; }
  public bool IsBothWaysTraffic { get; set; }
}

public sealed class MapEditorViewModel : FormViewModel
{
  public List<WaypointListDto> Waypoints { get; set; } = [];

  public List<WaypointLinkDto> WaypointLinks { get; set; } = [];

  public List<WaypointLinkDisplay> WaypointLinksDisplay { get; set; } = [];
}

public static class MapEditorViewModelExtensions
{
  public static void FromDto(this MapEditorViewModel mapEditorViewModel, MapEditorDto mapEditorDto)
  {
    // Waypoints
    mapEditorViewModel.Waypoints = mapEditorDto.Waypoints ?? [];

    // Waypoint Links
    mapEditorViewModel.WaypointLinks = mapEditorDto.WaypointLinks ?? [];

    // Key: (WaypointFromId, WaypointToId), Value: IsBothWaysTraffic
    Dictionary<(int, int), bool> waypointLinksDisplayTemp = [];
    foreach (var link in mapEditorDto.WaypointLinks!)
    {
      // Display
      // If other way around is exists, then it is both ways traffic
      if (waypointLinksDisplayTemp.ContainsKey(((int)link.WaypointToId!, (int)link.WaypointFromId!)))
      {
        waypointLinksDisplayTemp[((int)link.WaypointToId!, (int)link.WaypointFromId!)] = true;
      }
      else
      {
        waypointLinksDisplayTemp.Add(((int)link.WaypointFromId!, (int)link.WaypointToId!), false);
      }
    }
    // waypointLinksDisplayTemp to waypointLinksDisplay
    mapEditorViewModel.WaypointLinksDisplay = waypointLinksDisplayTemp.Select(x => new WaypointLinkDisplay
    {
      WaypointFromId = x.Key.Item1,
      WaypointToId = x.Key.Item2,
      IsBothWaysTraffic = x.Value
    }).ToList();
  }
}