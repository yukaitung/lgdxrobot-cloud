using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record MapEditorBusinessModel
{
  public IEnumerable<WaypointListBusinessModel> Waypoints { get; set; } = [];

  public IEnumerable<WaypointTrafficBusinessModel> WaypointTraffics { get; set; } = [];
}

public static class MapEditorBusinessModelExtensions
{
  public static MapEditorDto ToDto(this MapEditorBusinessModel model)
  {
    return new MapEditorDto
    {
      Waypoints = model.Waypoints.Select(w => w.ToDto()),
      WaypointTraffics = model.WaypointTraffics.Select(w => w.ToDto()),
    };
  }
}