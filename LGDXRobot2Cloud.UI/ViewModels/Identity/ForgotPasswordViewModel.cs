using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

public sealed record ForgotPasswordViewModel
{
  [Required (ErrorMessage = "Please enter an email address.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email address.")]
  public string Email { get; set; } = null!;

  public IDictionary<string,string[]>? Errors { get; set; }

  public bool IsSuccess { get; set; } = false;
}