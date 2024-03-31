using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class RobotTaskWaypointDetailCreateDto
  {
    [Required]
    public required int WaypointId { get; set; }

    [Required]
    public required int Order { get; set; }
  }
}