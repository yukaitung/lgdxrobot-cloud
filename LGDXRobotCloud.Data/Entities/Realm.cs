using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LGDXRobotCloud.Utilities.Constants;

namespace LGDXRobotCloud.Data.Entities;

[Table("Navigation.Realms")]
public class Realm
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Description { get; set; }

  [Required]
  public bool HasWaypointsTrafficControl { get; set; }

  [MaxLength(LgdxApiConstants.ImageMaxSize)]
  public byte[] Image { get; set; } = [];

  public double Resolution { get; set; } = 0;

  public double OriginX { get; set; } = 0;

  public double OriginY { get; set; } = 0;

  public double OriginRotation { get; set; } = 0;
}