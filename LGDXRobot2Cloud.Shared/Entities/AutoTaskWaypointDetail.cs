using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Shared.Entities
{
  public class AutoTaskWaypointDetail
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Order { get; set; }

    [ForeignKey("WaypointId")]
    public required Waypoint Waypoint { get; set; }

    public int WaypointId { get; set; }

    [ForeignKey("AutoTaskId")]
    public required AutoTask AutoTask { get; set; }

    public int AutoTaskId { get; set; }
  }
}