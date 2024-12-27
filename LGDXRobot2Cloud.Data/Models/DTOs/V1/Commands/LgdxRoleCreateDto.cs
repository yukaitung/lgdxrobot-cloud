using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record LgdxRoleCreateDto
{
  [Required]
  public required string Name { get; set; }

  public string? Description { get; set; }

  public IEnumerable<string> Scopes { get; set; } = []; // Empty scope is allowed
}