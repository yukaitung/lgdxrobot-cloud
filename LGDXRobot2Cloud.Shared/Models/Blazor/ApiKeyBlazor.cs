using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class ApiKeyBlazor : ApiKeyBaseDto
  {
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsUpdate { get; set; } = false;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (!IsUpdate && IsThirdParty && string.IsNullOrWhiteSpace(Secret))
      {
        yield return new ValidationResult("Secret is required for Third-Party API Key.", ["Secret"]);
      }
    }
  }
}