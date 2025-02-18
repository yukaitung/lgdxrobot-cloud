namespace LGDXRobotCloud.Data.Models.Emails;

public record WelcomePasswordSetViewModel
{
  public required string UserName { get; set; }

  public required string Email { get; set; }

  public required string Token { get; set; }
}