using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Constants;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record RealmMapUpdateDto
{
  // All fields are required in SLAM mode
  
  [MaxLength(LgdxApiConstants.ImageMaxSize, ErrorMessage = "The image size is too large.")]
  [Required(ErrorMessage = "Please upload an image.")]
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

public static class RealmMapUpdateDtoExtensions
{
  public static RealmMapUpdateBusinessModel ToBusinessModel(this RealmMapUpdateDto model)
  {
    return new RealmMapUpdateBusinessModel {
      Image = model.Image,
      Resolution = model.Resolution,
      OriginX = model.OriginX,
      OriginY = model.OriginY,
      OriginRotation = model.OriginRotation,
    };
  }
}