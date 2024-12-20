using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Base;
public record MapBaseDto
{
  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Description { get; set; }
  
  [Required]
  public string Image { get; set; } = null!;

  [Required]
  public double Resolution { get; set; }

  [Required]
  public double OriginX { get; set; }

  [Required]
  public double OriginY { get; set; }
}