using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record ForgotPasswordRequestDto
{
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public required string Email { get; set; }
}