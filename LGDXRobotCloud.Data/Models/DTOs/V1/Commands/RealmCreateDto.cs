using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

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

public static class RealmCreateDtoExtensions
{
  public static RealmCreateBusinessModel ToBusinessModel(this RealmCreateDto model)
  {
    return new RealmCreateBusinessModel {
      Name = model.Name,
      Description = model.Description,
      Image = model.Image,
      Resolution = model.Resolution,
      OriginX = model.OriginX,
      OriginY = model.OriginY,
      OriginRotation = model.OriginRotation,
    };
  }
}