namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record LgdxRoleDto
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }

  public string? Description { get; set; }

  public IEnumerable<string> Scopes { get; set; } = [];
}