namespace LGDXRobotCloud.Data.Models.Business.Identity;

public record LoginRequestBusinessModel
{
  public required string Username { get; set; }

  public required string Password { get; set; }
}