namespace LGDXRobot2Cloud.Data.Models.Identity;

public class LgdxUserListDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string UserName { get; set; } = null!;
  public bool TwoFactorEnabled { get; set; }
  public int AccessFailedCount { get; set; }
}