using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.Identity;

public class UpdatePasswordRequest
{
  [Required]
  public string OldPassword { get; set; } = null!;

  [Required]
  public string NewPassword { get; set; } = null!;
}