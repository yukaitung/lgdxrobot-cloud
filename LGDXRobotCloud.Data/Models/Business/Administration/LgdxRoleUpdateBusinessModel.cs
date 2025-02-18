namespace LGDXRobotCloud.Data.Models.Business.Administration;

public class LgdxRoleUpdateBusinessModel
{
  public required string Name { get; set; }

  public string? Description { get; set; }

  public IEnumerable<string> Scopes { get; set; } = [];
}