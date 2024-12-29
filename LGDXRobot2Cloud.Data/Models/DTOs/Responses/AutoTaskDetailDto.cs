using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class AutoTaskDetailDto
{
  public int Id { get; set; }
  public int Order { get; set; }
  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  public WaypointDto? Waypoint { get; set; }
}
