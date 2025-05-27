using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobotCloud.Data.Entities;

[Table("Navigation.WaypointLinks")]
public class WaypointLink
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [ForeignKey("RealmId")]
  public Realm Realm { get; set; } = null!;

  [Required]
  public int RealmId { get; set; }

  [ForeignKey("WaypointFromId")]
  public Waypoint WaypointFrom { get; set; } = null!;

  [Required]
  public int WaypointFromId { get; set; }

  [ForeignKey("WaypointToId")]
  public Waypoint WaypointTo { get; set; } = null!;

  [Required]
  public int WaypointToId { get; set; }
}
