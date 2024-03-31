using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class RobotTaskWaypointDetail
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Order { get; set; }

    [ForeignKey("WaypointId")]
    public required Waypoint Waypoint { get; set; }

    public int WaypointId { get; set; }

    [ForeignKey("RobotTaskId")]
    public required RobotTask RobotTask { get; set; }

    public int RobotTaskId { get; set; }
  }
}