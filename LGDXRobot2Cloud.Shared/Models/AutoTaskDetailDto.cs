namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskDetailDto
  {
    public int Id { get; set; }
    public int Order { get; set; }
    public double? CustomX { get; set; }

    public double? CustomY { get; set; }

    public double? CustomRotation { get; set; }
    public WaypointDto? Waypoint { get; set; }
  }
}