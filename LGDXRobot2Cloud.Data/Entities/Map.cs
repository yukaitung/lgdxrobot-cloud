using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Navigation.Maps")]
public class Map
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

  [Precision(3)]
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
  [Precision(3)]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}