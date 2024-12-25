namespace LGDXRobot2Cloud.UI.Models;

public class ResetPasswordRequest
{
  public string NewPassword { get; set; } = null!;
  public string ConfirmPassword { get; set; } = null!;
}