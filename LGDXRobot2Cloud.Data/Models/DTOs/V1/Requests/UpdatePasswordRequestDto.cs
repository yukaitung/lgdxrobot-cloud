using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record UpdatePasswordRequestDto
{
  [Required (ErrorMessage = "Please enter a current password.")]
  public required string CurrentPassword { get; set; }

  [Required (ErrorMessage = "Please enter a new password.")]
  public required string NewPassword { get; set; }
}