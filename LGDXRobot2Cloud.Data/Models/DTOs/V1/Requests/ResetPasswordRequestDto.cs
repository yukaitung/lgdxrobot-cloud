using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record ResetPasswordRequestDto
{
  [Required (ErrorMessage = "Please enter a username.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public required string Email { get; set; }

  [Required (ErrorMessage = "Please enter a token.")]
  public required string Token { get; set; }

  [Required (ErrorMessage = "Please enter a new password.")]
  public required string NewPassword { get; set; }
}