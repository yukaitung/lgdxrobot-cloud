using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public sealed class ForgotPasswordViewModel : FormViewModel
{
  [Required (ErrorMessage = "Please enter an email address.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email address.")]
  public string Email { get; set; } = null!;
}

public static class ForgotPasswordViewModelExtensions
{
  public static ForgotPasswordRequestDto ToForgotPasswordRequestDto(this ForgotPasswordViewModel forgotPasswordViewModel)
  {
    return new ForgotPasswordRequestDto {
      Email = forgotPasswordViewModel.Email
    };
  }
}