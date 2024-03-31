using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class RobotTaskWaypointDetailUpdateDto
  {
    public int? Id { get; set; }

    [Required]
    public required int Order { get; set; }

    [Required]
    public required int WaypointId { get; set; }
  }
}