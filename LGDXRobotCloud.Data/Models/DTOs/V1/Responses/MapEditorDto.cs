namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record MapEditorDto
{
  public IEnumerable<WaypointListDto> Waypoints { get; set; } = [];

  public IEnumerable<WaypointTrafficDto> WaypointTraffics { get; set; } = [];
}