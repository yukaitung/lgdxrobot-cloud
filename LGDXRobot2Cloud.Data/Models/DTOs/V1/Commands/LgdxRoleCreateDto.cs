using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record LgdxRoleCreateDto : IValidatableObject
{
  [Required (ErrorMessage = "Please enter a name.")]
  public required string Name { get; set; }

  public string? Description { get; set; }

  public IEnumerable<string> Scopes { get; set; } = []; // Empty scope is allowed

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    int i = 0;
    foreach (var scope in Scopes)
    {
      if (string.IsNullOrWhiteSpace(scope))
      {
        yield return new ValidationResult($"Scope is required.", [nameof(Scopes), $"{nameof(Scopes)}-{i}"]);
      }
      i++;
    }
  }
}