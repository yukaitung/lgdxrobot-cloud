using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public record FlowDetailBody
{
  public int? Id { get; set; } = null;
  
  public int Order { get; set; }

  [Required (ErrorMessage = "Please select a progress.")]
  public int? ProgressId { get; set; } = null;

  public string? ProgressName { get; set; } = null;

  public int AutoTaskNextControllerId { get; set; } = 1;

  public int? TriggerId { get; set; } = null;

  public string? TriggerName { get; set; } = null;
}

public class FlowDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public List<FlowDetailBody> FlowDetails { get; set; } = [];

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    foreach (var flow in FlowDetails)
    {
      List<ValidationResult> validationResults = [];
      var vc = new ValidationContext(flow);
      Validator.TryValidateObject(flow, vc, validationResults, true);
      foreach (var validationResult in validationResults)
      {
        yield return validationResult;
      }
    }
  }
}