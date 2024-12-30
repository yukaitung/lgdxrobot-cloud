using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public record TaskDetailBody : IValidatableObject
{
  public int? Id { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public int? WaypointId { get; set; }

  public string? WaypointName { get; set; }

  public int Order { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (WaypointId == null && CustomX == null && CustomY == null && CustomRotation == null)
    {
      yield return new ValidationResult("Please enter a waypoint or a custom coordinate.", [nameof(AutoTaskDetailViewModel.AutoTaskDetails)]);
    }
  }
}

public class AutoTaskDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  public string? Name { get; set; }

  public List<TaskDetailBody> AutoTaskDetails { get; set; } = [];

  [Required (ErrorMessage = "Please enter a priority.")]
  public int Priority { get; set; } = 0;

  [Required (ErrorMessage = "Please select a Flow.")]
  public int? FlowId { get; set; } = null;

  public string? FlowName { get; set; } = null;

  [Required (ErrorMessage = "Please select a Realm.")]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; } = null;

  public Guid? AssignedRobotId { get; set; } = null;

  public string? AssignedRobotName { get; set; } = null;

  public int CurrentProgressId { get; set; }

  public string CurrentProgressName { get; set; } = string.Empty;

  public bool IsTemplate { get; set; } = false;

  public bool IsClone { get; set; } = false;

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