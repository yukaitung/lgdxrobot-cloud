using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobotCloud.Data.Entities;

[Table("Navigation.Waypoints")]
public class Waypoint
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(100)]
  [Required]
  public string Name { get; set; } = null!;

  [ForeignKey("RealmId")]
  public Realm Realm { get; set; } = null!;

  [Required]
  public int RealmId { get; set; }
  
  [Required]
  public double X { get; set; }

  [Required]
  public double Y { get; set; }

  [Required]
  public double Rotation { get; set; }

  [Required]
  public bool IsParking { get; set; }

  [Required]
  public bool HasCharger { get; set; }

  [Required]
  public bool IsReserved { get; set; }
}
