using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Identity;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Constants;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record RealmUpdateDto
{
  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  [MaxLength(200)]
  public string? Description { get; set; }
  
  [Required(ErrorMessage = "Please select a traffic control type.")]
  public bool HasWaypointsTrafficControl { get; set; }
  
  [MaxLength(LgdxApiConstants.ImageMaxSize, ErrorMessage = "The image size is too large.")]
  public string? Image { get; set; }

  public double Resolution { get; set; }

  public double OriginX { get; set; }

  public double OriginY { get; set; }

  public double OriginRotation { get; set; }
}

public static class RealmUpdateDtoExtensions
{
  public static RealmUpdateBusinessModel ToBusinessModel(this RealmUpdateDto model)
  {
    return new RealmUpdateBusinessModel {
      Name = model.Name,
      Description = model.Description,
      HasWaypointsTrafficControl = model.HasWaypointsTrafficControl,
      Image = model.Image,
      Resolution = model.Resolution,
      OriginX = model.OriginX,
      OriginY = model.OriginY,
      OriginRotation = model.OriginRotation,
    };
  }
}