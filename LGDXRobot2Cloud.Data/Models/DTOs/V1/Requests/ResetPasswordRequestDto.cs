using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record ResetPasswordRequestDto
{
  [Required]
  public required string Email { get; set; }

  [Required]
  public required string Token { get; set; }

  [Required]
  public required string NewPassword { get; set; }
}