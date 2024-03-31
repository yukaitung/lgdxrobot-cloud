using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class RobotTaskUpdateDto
  {
    public string? Name { get; set; }
    public IEnumerable<RobotTaskWaypointDetailUpdateDto> Waypoints { get; set; } = new List<RobotTaskWaypointDetailUpdateDto>();
    public int Priority { get; set; }
   
   [Required]
   public required int FlowId { get; set; }
  }
}