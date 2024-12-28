using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration.Roles;

public sealed class RolesDetailViewModel : FormViewModel, IValidatableObject
{
  public Guid Id { get; set; }
  
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public List<string> Scopes { get; set; } = [];

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    int i = 0;
    foreach (var scope in Scopes)
    {
      if (string.IsNullOrWhiteSpace(scope))
      {
        yield return new ValidationResult($"Scope is required.", [$"{nameof(Scopes)}-{i}"]);
      }
      i++;
    }
  }
}