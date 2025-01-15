using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Automation;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record AutoTaskDetailUpdateDto : IValidatableObject
{
  public int? Id { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public int? WaypointId { get; set; }

  [Required (ErrorMessage = "Please enter the order.")]
  public int Order { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (WaypointId == null && CustomX == null && CustomY == null && CustomRotation == null)
    {
      yield return new ValidationResult("Please enter a waypoint or a custom coordinate.", [nameof(AutoTaskUpdateDto.AutoTaskDetails)]);
    }
  }
}

public static class AutoTaskDetailUpdateDtoExtensions
{
  public static AutoTaskDetailUpdateBusinessModel ToBusinessModel(this AutoTaskDetailUpdateDto model)
  {
    return new AutoTaskDetailUpdateBusinessModel {
      Id = model.Id,
      CustomX = model.CustomX,
      CustomY = model.CustomY,
      CustomRotation = model.CustomRotation,
      WaypointId = model.WaypointId,
      Order = model.Order,
    };
  }
}