namespace LGDXRobot2Cloud.Data.Models.Identity;

public class LgdxRoleUpdateDto
{
  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public IEnumerable<string> Scopes { get; set; } = [];
}