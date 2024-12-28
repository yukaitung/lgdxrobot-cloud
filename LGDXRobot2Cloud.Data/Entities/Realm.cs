using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Data.Entities;

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

  [Column(TypeName = "MEDIUMBLOB")]
  [Required]
  public byte[] Image { get; set; } = null!;

  [Required]
  public double Resolution { get; set; }

  [Required]
  public double OriginX { get; set; }

  [Required]
  public double OriginY { get; set; }

  [Required]
  public double OriginRotation { get; set; }
}