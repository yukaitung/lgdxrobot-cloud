namespace LGDXRobot2Cloud.Data.Models.Identity;

public class LgdxRoleDto
{
  public Guid Id { get; set; } 
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public IEnumerable<string> Scopes { get; set; } = [];
}