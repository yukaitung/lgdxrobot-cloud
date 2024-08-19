using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Base;

public class ApiKeyBaseDto : IValidatableObject
{
  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Secret { get; set; }

  [Required]
  public bool IsThirdParty { get; set; }

  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (IsThirdParty && string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("Secret is required for Third-Party API Key.", ["Secret"]);
    }
    if (!IsThirdParty && !string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("LGDXRobot2 API Keys will be generated automatically.", ["Secret"]);
    }
  }
}
