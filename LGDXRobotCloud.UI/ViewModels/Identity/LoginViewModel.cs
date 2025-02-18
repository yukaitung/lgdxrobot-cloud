using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Identity;

public sealed class LoginViewModel : FormViewModel
{
  [Required (ErrorMessage = "Please enter a username.")]
  public string Username { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a password.")]
  public string Password { get; set; } = null!;
}

public static class LoginViewModelExtensions
{
  public static LoginRequestDto ToLoginRequestDto(this LoginViewModel loginViewModel)
  {
    return new LoginRequestDto {
      Username = loginViewModel.Username,
      Password = loginViewModel.Password
    };
  }
}