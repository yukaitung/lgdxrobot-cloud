namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskWaypointDetailDto
  {
    public int? Id { get; set; }
    public int Order { get; set; }
    public required WaypointDto Waypoint { get; set; }
  }
}