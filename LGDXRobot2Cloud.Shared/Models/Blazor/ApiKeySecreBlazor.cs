using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class ApiKeySecretBlazor : IValidatableObject
  {
    public bool IsThirdParty { get; set; }

    [MaxLength(200)]
    public string Secret { get; set; } = null!;

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (IsThirdParty && string.IsNullOrWhiteSpace(Secret))
      {
        yield return new ValidationResult("Secret is required for Third-Party API Key.", ["Secret"]);
      }
    }
  }
}