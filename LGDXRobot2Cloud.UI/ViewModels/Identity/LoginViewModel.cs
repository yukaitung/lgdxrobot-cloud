using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.Client.Models;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

public sealed record LoginViewModel
{
  [Required (ErrorMessage = "Please enter a username.")]
  public string Username { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a password.")]
  public string Password { get; set; } = null!;

  public IDictionary<string,string[]>? Errors { get; set; }
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