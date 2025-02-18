namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record LgdxUserUpdateAdminBusinessModel
{
  public required string Name { get; set; }

  public required string UserName { get; set; }

  public required string Email { get; set; }

  public IEnumerable<string> Roles { get; set; } = [];
}