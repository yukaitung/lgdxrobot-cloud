using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Automation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record AutoTaskDetailCreateDto : IValidatableObject
{
  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public int? WaypointId { get; set; }

  [Required (ErrorMessage = "Please enter the order.")]
  public required int Order { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (WaypointId == null && CustomX == null && CustomY == null && CustomRotation == null)
    {
      yield return new ValidationResult("Please enter a waypoint or a custom coordinate.", [nameof(AutoTaskCreateDto.AutoTaskDetails)]);
    }
  }
}

public static class AutoTaskDetailCreateDtoExtensions
{
  public static AutoTaskDetailCreateBusinessModel ToBusinessModel(this AutoTaskDetailCreateDto model)
  {
    return new AutoTaskDetailCreateBusinessModel {
      CustomX = model.CustomX,
      CustomY = model.CustomY,
      CustomRotation = model.CustomRotation,
      WaypointId = model.WaypointId,
      Order = model.Order,
    };
  }
}