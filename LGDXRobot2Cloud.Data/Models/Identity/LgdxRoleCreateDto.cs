using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.Identity;

public class LgdxRoleCreateDto
{
  [Required]
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public IEnumerable<string> Scopes { get; set; } = [];
}