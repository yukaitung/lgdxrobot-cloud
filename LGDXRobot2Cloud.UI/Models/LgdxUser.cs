namespace LGDXRobot2Cloud.UI.Models;

public class LgdxUser
{
  public Guid Id { get; set; }
  public string UserName { get; set; } = null!;
  public string Name { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? Password { get; set; }
  public List<string> Roles { get; set; } = [];
  public bool TwoFactorEnabled { get; set; }
  public int AccessFailedCount { get; set; }
}