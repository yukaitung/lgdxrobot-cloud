using LGDXRobot2Cloud.API.Utilities;

namespace LGDXRobot2Cloud.API.Models
{
  public class RobotTaskCreateDto
  {
    public string? Name { get; set; }
    public IEnumerable<int> Waypoints { get; set; } = new List<int>();
    public int Priority { get; set; }
    public int FlowId { get; set; }
    public readonly int CurrentProgressId = (int)ProgressState.Waiting;
  }
}