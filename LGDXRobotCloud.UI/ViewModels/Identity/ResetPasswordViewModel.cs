using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public class ResetPasswordViewModel : FormViewModel, IValidatableObject
{
  public string Email = null!;

	public string Token { get; set; } = null!;

  [Required]
	public string NewPassword { get; set; } = null!;

  [Required]
	public string ConfirmPassword { get; set; } = null!;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (NewPassword != ConfirmPassword)
    {
      yield return new ValidationResult("The new password dos not match.", [nameof(NewPassword)]);
      yield return new ValidationResult("The new password dos not match.", [nameof(ConfirmPassword)]);
    }
  }
}

public static class ResetPasswordViewModelExtensions
{
  public static ResetPasswordRequestDto ToResetPasswordRequestDto(this ResetPasswordViewModel resetPasswordViewModel)
  {
    return new ResetPasswordRequestDto {
      Email = resetPasswordViewModel.Email,
      Token = resetPasswordViewModel.Token,
      NewPassword = resetPasswordViewModel.NewPassword
    };
  }
}