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
    public Waypoint Waypoint { get; set; } = null!;

    public int WaypointId { get; set; }

    [ForeignKey("AutoTaskId")]
    public AutoTask AutoTask { get; set; } = null!;

    public int AutoTaskId { get; set; }
  }
}