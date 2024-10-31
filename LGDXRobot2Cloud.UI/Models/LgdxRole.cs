namespace LGDXRobot2Cloud.UI.Models;

public class LgdxRole
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public List<string> Scopes { get; set; } = [];
}