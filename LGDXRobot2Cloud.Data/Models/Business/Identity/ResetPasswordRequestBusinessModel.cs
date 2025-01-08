namespace LGDXRobot2Cloud.Data.Models.Business.Identity;

public record ResetPasswordRequestBusinessModel
{
  public required string Email { get; set; }

  public required string Token { get; set; }

  public required string NewPassword { get; set; }
}