using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record RealmCreateDto
{
  [MaxLength(50)]
  [Required]
  public required string Name { get; set; }

  [MaxLength(200)]
  public string? Description { get; set; }
  
  [Required]
  public required string Image { get; set; }

  [Required]
  public required double Resolution { get; set; }

  [Required]
  public required double OriginX { get; set; }

  [Required]
  public required double OriginY { get; set; }

  [Required]
  public required double OriginRotation { get; set; }
}