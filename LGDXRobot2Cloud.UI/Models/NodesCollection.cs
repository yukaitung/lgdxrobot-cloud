using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class NodesCollection : IValidatableObject
{
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = null!;
  
  [Required]
  public IList<NodesCollectionDetail> Nodes { get; set; } = [];

  public DateTime CreatedAt { get; set; }
  
  public DateTime UpdatedAt { get; set; }

  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Nodes.Count == 0 || (Nodes.Count == 1 && Nodes[0].NodeId == null))
    {
      yield return new ValidationResult("At least one node is required.", [nameof(Nodes)]);
    }
  }
}
