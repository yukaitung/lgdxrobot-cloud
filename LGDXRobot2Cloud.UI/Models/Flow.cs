using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.UI.Models;

public class Flow : IValidatableObject
{
  public int Id { get; set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  public IList<FlowDetail> FlowDetails { get; set; } = [];

  public DateTime CreatedAt { get; set; }
  
  public DateTime UpdatedAt { get; set; }

  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (FlowDetails.Count == 0 || (FlowDetails.Count == 1 && FlowDetails[0].ProgressId == null))
    {
      yield return new ValidationResult("At least one step is required.", [nameof(FlowDetails)]);
    }
    for (int i = 0; i < FlowDetails.Count; i++)
    {
      if (FlowDetails[i].AutoTaskNextControllerId == (int)AutoTaskNextController.API && FlowDetails[i].TriggerId == null)
      {
        yield return new ValidationResult($"The Begin Trigger is requried step {i + 1}.", [nameof(FlowDetails)]);
      }
    }
  }
}
