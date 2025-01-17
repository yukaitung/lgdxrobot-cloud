namespace LGDXRobot2Cloud.Data.Models.Business.Identity;

public record LgdxUserUpdateBusinessModel
{
  public required string Name { get; set; }

  public required string Email { get; set; }
}