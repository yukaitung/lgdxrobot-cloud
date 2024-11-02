using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.Identity;

public class LgdxUserCreateDto
{
  [Required]
  public string Name { get; set; } = null!;

  [Required]
  public string UserName { get; set; } = null!;

  [Required]
  public string Email { get; set; } = null!;

  public string? Password { get; set; }

  public IEnumerable<string> Roles { get; set; } = [];
}