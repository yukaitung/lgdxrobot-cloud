using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class ApiKey : IValidatableObject
{
  public int Id { get; set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Secret { get; set; }

  [Required]
  public bool IsThirdParty { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }

  public bool IsUpdate { get; set; } = false;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (!IsUpdate && IsThirdParty && string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("Secret is required for Third-Party API Key.", [nameof(Secret)]);
    }
  }
}
