using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.Identity;

public class LoginRequest
{
  [Required]
  public string Username { get; set; } = null!;

  [Required]
  public string Password { get; set; } = null!;
}