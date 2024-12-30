using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration;

public class UserDetailViewModel : FormViewModel, IValidatableObject
{
  public Guid Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a username.")]
  public string UserName { get; set; } = null!;

  [Required (ErrorMessage = "Please enter an email.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public string Email { get; set; } = null!;

  public string Password { get; set; } = null!;

  public List<string> Roles { get; set; } = [];

  public bool TwoFactorEnabled { get; set; }
  
  public int AccessFailedCount { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Roles.Count == 0)
    {
      yield return new ValidationResult("At least one role is required.", [nameof(Roles)]);
    }
    int i = 0;
    foreach (var role in Roles)
    {
      if (string.IsNullOrWhiteSpace(role))
      {
        yield return new ValidationResult($"Role is required.", [$"{nameof(Roles)}-{i}"]);
      }
      i++;
    }
  }
}