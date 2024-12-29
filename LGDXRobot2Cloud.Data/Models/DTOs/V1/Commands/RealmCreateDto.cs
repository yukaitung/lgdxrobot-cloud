using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record RealmCreateDto
{
  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [MaxLength(200)]
  public string? Description { get; set; }
  
  [Required (ErrorMessage = "Please upload an image.")]
  public required string Image { get; set; }

  [Required (ErrorMessage = "Please enter a resolution.")]
  public required double Resolution { get; set; }

  [Required (ErrorMessage = "Please enter an origin X coordinate.")]
  public required double OriginX { get; set; }

  [Required (ErrorMessage = "Please enter an origin Y coordinate.")]
  public required double OriginY { get; set; }

  [Required (ErrorMessage = "Please enter an origin rotation.")]
  public required double OriginRotation { get; set; }
}