namespace LGDXRobotCloud.Data.Models.Emails;

public record PasswordUpdateViewModel
{
  public required string UserName { get; set; }

  public required string Time { get; set; }
}