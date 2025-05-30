using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record MapEditorBusinessModel
{
  public IEnumerable<WaypointListBusinessModel> Waypoints { get; set; } = [];

  public IEnumerable<WaypointLinkBusinessModel> WaypointLinks { get; set; } = [];
}

public static class MapEditorBusinessModelExtensions
{
  public static MapEditorDto ToDto(this MapEditorBusinessModel model)
  {
    return new MapEditorDto
    {
      Waypoints = model.Waypoints.Select(w => w.ToDto()),
      WaypointLinks = model.WaypointLinks.Select(w => w.ToDto()),
    };
  }
}