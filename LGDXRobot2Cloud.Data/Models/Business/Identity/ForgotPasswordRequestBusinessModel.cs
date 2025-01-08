namespace LGDXRobot2Cloud.Data.Models.Business.Identity;

public record ForgotPasswordRequestBusinessModel
{
  public required string Email { get; set; }
}