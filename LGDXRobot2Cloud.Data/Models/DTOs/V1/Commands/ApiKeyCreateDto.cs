using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Administration;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record ApiKeyCreateDto
{
  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public required string Name { get; set; }

  [MaxLength(200)]
  public string? Secret { get; set; }

  [Required]
  public required bool IsThirdParty { get; set; }

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

public static class ApiKeyCreateDtoExtensions
{
  public static ApiKeyCreateBusinessModel ToBusinessModel(this ApiKeyCreateDto apiKey)
  {
    return new ApiKeyCreateBusinessModel{
      Name = apiKey.Name,
      Secret = apiKey.Secret,
      IsThirdParty = apiKey.IsThirdParty
    };
  }
}