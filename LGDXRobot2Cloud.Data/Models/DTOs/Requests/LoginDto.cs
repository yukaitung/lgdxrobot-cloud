using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Requests;

public class LoginDto
{
  [Required]
  public string Username { get; set; } = null!;

  [Required]
  public string Password { get; set; } = null!;
}