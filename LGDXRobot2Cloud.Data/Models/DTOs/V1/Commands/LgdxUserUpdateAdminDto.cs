using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record LgdxUserUpdateAdminDto
{
  [Required]
  public required string Name { get; set; }

  [Required]
  public required string UserName { get; set; }

  [Required]
  public required string Email { get; set; }

  public IEnumerable<string> Roles { get; set; } = [];
}