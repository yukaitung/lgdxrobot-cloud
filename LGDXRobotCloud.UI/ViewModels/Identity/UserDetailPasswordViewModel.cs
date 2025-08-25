using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public class UserDetailPasswordViewModel : FormViewModel, IValidatableObject
{
  [Required (ErrorMessage = "Please enter a current password.")]
  public string CurrentPassword { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a new password.")]
  public string NewPassword { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a confirm password.")]
  public string ConfirmPassword { get; set; } = null!;

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (CurrentPassword == NewPassword)
    {
      yield return new ValidationResult("The new password cannot be the same as the current password.", [nameof(CurrentPassword), nameof(NewPassword)]);
    }
    if (NewPassword != ConfirmPassword)
    {
      yield return new ValidationResult("The new password and confirm password do not match.", [nameof(NewPassword), nameof(ConfirmPassword)]);
    }
  }
}

public static class UserDetailPasswordViewModelExtensions
{
  public static UpdatePasswordRequestDto ToUpdateDto(this UserDetailPasswordViewModel userDetailPasswordViewModel)
  {
    return new UpdatePasswordRequestDto {
      CurrentPassword = userDetailPasswordViewModel.CurrentPassword,
      NewPassword = userDetailPasswordViewModel.NewPassword,
    };
  }
}