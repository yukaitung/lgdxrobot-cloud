using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Automation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record AutoTaskUpdateDto : IValidatableObject
{
  public string? Name { get; set; }

  public required IEnumerable<AutoTaskDetailUpdateDto> AutoTaskDetails { get; set; } = [];
  
  [Required (ErrorMessage = "Please enter a priority.")]
  public required int Priority { get; set; }
  
  [Required (ErrorMessage = "Please select a Flow.")]
  public required int FlowId { get; set; }

  public Guid? AssignedRobotId { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    foreach (var autoTaskDetail in AutoTaskDetails)
    {
      List<ValidationResult> validationResults = [];
      var vc = new ValidationContext(autoTaskDetail);
      Validator.TryValidateObject(autoTaskDetail, vc, validationResults, true);
      foreach (var validationResult in validationResults)
      {
        yield return validationResult;
      }
    }
  }
}

public static class AutoTaskUpdateDtoExtensions
{
  public static AutoTaskUpdateBusinessModel ToBusinessModel(this AutoTaskUpdateDto model)
  {
    return new AutoTaskUpdateBusinessModel {
      Name = model.Name,
      AutoTaskDetails = model.AutoTaskDetails.Select(td => td.ToBusinessModel()),
      Priority = model.Priority,
      FlowId = model.FlowId,
      AssignedRobotId = model.AssignedRobotId,
    };
  }
}
