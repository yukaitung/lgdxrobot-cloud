using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

public sealed record LoginViewModel
{
  [Required (ErrorMessage = "Please enter a username.")]
  public string Username { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a password.")]
  public string Password { get; set; } = null!;

  public IDictionary<string,string[]>? Errors { get; set; }
}