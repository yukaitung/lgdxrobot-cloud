using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

public sealed class ResetPasswordViewModel : IValidatableObject
{
  public string Email = null!;

	public string Token { get; set; } = null!;

  [Required]
	public string NewPassword { get; set; } = null!;

  [Required]
	public string ConfirmPassword { get; set; } = null!;

	public IDictionary<string,string[]>? Errors { get; set; }

	public bool IsSuccess { get; set; } = false;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (NewPassword != ConfirmPassword)
    {
      yield return new ValidationResult("The new password dos not match.", [nameof(NewPassword)]);
      yield return new ValidationResult("The new password dos not match.", [nameof(ConfirmPassword)]);
    }
  }
}