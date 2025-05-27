using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record MapEditBusinessModel
{
  public IEnumerable<WaypointBusinessModel> Waypoints { get; set; } = [];

  public IEnumerable<WaypointLinkBusinessModel> WaypointLinks { get; set; } = [];
}

public static class MapEditBusinessModelExtensions
{
  public static MapEditDto ToDto(this MapEditBusinessModel model)
  {
    return new MapEditDto
    {
      Waypoints = model.Waypoints.Select(w => w.ToDto()),
      WaypointLinks = model.WaypointLinks.Select(w => w.ToDto()),
    };
  }
}