namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record LgdxUserCreateAdminBusinessModel
{
  public required string Name { get; set; }

  public required string UserName { get; set; }

  public required string Email { get; set; }

  public string? Password { get; set; }

  public IEnumerable<string> Roles { get; set; } = [];
}