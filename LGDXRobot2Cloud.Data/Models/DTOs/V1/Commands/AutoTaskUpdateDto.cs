using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record AutoTaskUpdateDto : IValidatableObject
{
  public string? Name { get; set; }

  public required IEnumerable<AutoTaskDetailUpdateDto> AutoTaskDetails { get; set; } = [];
  
  [Required (ErrorMessage = "Please enter a priority.")]
  public required int Priority { get; set; }
  
  [Required (ErrorMessage = "Please select a Flow.")]
  public required int FlowId { get; set; }

  [Required (ErrorMessage = "Please select a Realm.")]
  public required int RealmId { get; set; }

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
