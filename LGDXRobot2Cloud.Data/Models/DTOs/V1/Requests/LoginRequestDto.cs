using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record LoginRequestDto
{
  [Required]
  public required string Username { get; set; }

  [Required]
  public required string Password { get; set; }
}