namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record AutoTaskDetailDto
{
  public required int Id { get; set; }

  public required int Order { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public WaypointDto? Waypoint { get; set; }
}
