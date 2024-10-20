namespace LGDXRobot2Cloud.Data.Models.Identify;

public class LgdxUserDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string UserName { get; set; } = null!;
  public string Email { get; set; } = null!;
  public bool TwoFactorEnabled { get; set; }
  public int AccessFailedCount { get; set; }
}