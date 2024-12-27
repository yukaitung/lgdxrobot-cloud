using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

public sealed class ForgotPasswordViewModel
{
  [Required (ErrorMessage = "Please enter an email address.")]
  public string Email { get; set; } = null!;

  public IDictionary<string,string[]>? Errors { get; set; }

  public bool Success { get; set; } = false;
}