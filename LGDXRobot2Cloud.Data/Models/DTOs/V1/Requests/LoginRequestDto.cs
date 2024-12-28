using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record LoginRequestDto
{
  [Required (ErrorMessage = "Please enter a username.")]
  public required string Username { get; set; }

  [Required (ErrorMessage = "Please enter a password.")]
  public required string Password { get; set; }
}