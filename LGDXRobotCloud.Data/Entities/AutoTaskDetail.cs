using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Entities;

[Table("Automation.AutoTaskDetails")]
[Index(nameof(Order))]
public class AutoTaskDetail
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  public int Order { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }

  [ForeignKey("WaypointId")]
  public Waypoint? Waypoint { get; set; } = null!;

  public int? WaypointId { get; set; }

  [ForeignKey("AutoTaskId")]
  public AutoTask AutoTask { get; set; } = null!;

  public int AutoTaskId { get; set; }
}
