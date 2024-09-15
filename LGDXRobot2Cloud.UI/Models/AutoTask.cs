using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class AutoTask : IValidatableObject
{
  public int Id { get; set; }

  [MaxLength(50)]
  public string? Name { get; set; }

  public IList<AutoTaskDetail> Details { get; set; } = [];

  public int? Priority { get; set; }

  public Robot? AssignedRobot { get; set; }

  public Guid? AssignedRobotId { get; set; }

  public string? AssignedRobotName { get; set; }

  public Flow Flow { get; set; } = null!;
  
  [Required]
  public int? FlowId { get; set; }

  public string? FlowName { get; set; }

  public Progress CurrentProgress { get; set; } = null!;

  public bool IsTemplate { get; set; } = false;

  public DateTime CreatedAt { get; set; }
  
  public DateTime UpdatedAt { get; set; }

  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Details.Count == 0 || (Details.Count == 1 && 
          Details[0].CustomX == null && 
          Details[0].CustomY == null && 
          Details[0].CustomRotation == null && 
          Details[0].WaypointId == null))
    {
      yield return new ValidationResult("At least one wayopint is required.", [nameof(Details)]);
    }
  }
}
