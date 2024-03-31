using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.API.Utilities;

namespace LGDXRobot2Cloud.API.Models
{
  public class RobotTaskCreateDto
  {
    public string? Name { get; set; }
    public IEnumerable<RobotTaskWaypointDetailCreateDto> Waypoints { get; set; } = new List<RobotTaskWaypointDetailCreateDto>();
    public int Priority { get; set; }

    [Required]
    public required int FlowId { get; set; }
    public readonly int CurrentProgressId = (int)ProgressState.Waiting;
  }
}