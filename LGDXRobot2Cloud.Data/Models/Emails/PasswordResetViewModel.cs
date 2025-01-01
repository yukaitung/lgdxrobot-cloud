namespace LGDXRobot2Cloud.Data.Models.Emails;

public record PasswordResetViewModel
{
  public required string UserName { get; set; }

  public required string Email { get; set; }

  public required string Token { get; set; }
}