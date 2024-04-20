namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskWaypointDetailDto
  {
    public int Id { get; set; }
    public int Order { get; set; }
    public WaypointDto Waypoint { get; set; } = null!;
  }
}