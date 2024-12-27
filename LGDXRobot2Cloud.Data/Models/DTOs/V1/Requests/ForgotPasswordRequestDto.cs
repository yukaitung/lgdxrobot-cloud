namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record ForgotPasswordRequestDto
{
  public required string Email { get; set; }
}