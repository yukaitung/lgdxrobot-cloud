using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record UpdatePasswordRequestDto
{
  [Required]
  public required string CurrentPassword { get; set; }

  [Required]
  public required string NewPassword { get; set; }
}