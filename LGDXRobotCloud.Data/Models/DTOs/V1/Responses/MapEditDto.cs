namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record MapEditDto
{
  public IEnumerable<WaypointDto> Waypoints { get; set; } = [];

  public IEnumerable<WaypointLinkDto> WaypointLinks { get; set; } = [];
}