namespace LGDXRobot2Cloud.API.Models
{
  public class RobotTaskDto
  {
    public int Id { get; set; }
    public string? Name { get; set; }
    public IEnumerable<RobotTaskWaypointDetailDto> Waypoints { get; set; } = new List<RobotTaskWaypointDetailDto>();
    public int Priority { get; set; }
    public required FlowListDto Flow { get; set; }
    public required ProgressDto CurrentProgress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}