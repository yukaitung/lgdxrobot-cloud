using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Shared.Enums;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class FlowDetailBlazor : IValidatableObject
  {
    public int? Id { get; set; }

    // Will be added before submit
    public int Order { get; set; }

    public ProgressBlazor? Progress { get; set; }

    [Required]
    public int? ProgressId { get; set; }

    public string? ProgressName { get; set; }

    public int AutoTaskNextControllerId { get; set; } = (int)AutoTaskNextController.Robot;
  
    public TriggerBlazor? StartTrigger { get; set; }

    public int? StartTriggerId { get; set; }

    public string? StartTriggerName { get; set; }

    public TriggerBlazor? EndTrigger { get; set; }

    public int? EndTriggerId { get; set; }

    public string? EndTriggerName { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (AutoTaskNextControllerId == (int)AutoTaskNextController.API && StartTriggerId == null)
      {
        yield return new ValidationResult("The Begin Trigger is requried for condition in API.", ["StartTriggerId"]);
      }
    }
  }
}