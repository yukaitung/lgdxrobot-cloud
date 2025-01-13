using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

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