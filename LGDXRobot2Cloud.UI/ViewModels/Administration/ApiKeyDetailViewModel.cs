using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration;

public class ApiKeyDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Secret { get; set; } = null!;

  public bool IsThirdParty { get; set; }

  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (IsThirdParty && string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("Secret is required for Third-Party API Key.", [nameof(Secret)]);
    }
    if (!IsThirdParty && !string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("LGDXRobot2 API Keys will be generated automatically.", [nameof(Secret)]);
    }
  }
}