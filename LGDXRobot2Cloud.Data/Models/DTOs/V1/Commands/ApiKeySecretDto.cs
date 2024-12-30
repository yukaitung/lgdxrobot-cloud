using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record ApiKeySecretUpdateDto
{
  [Required]
  [MaxLength(200)]
  public required string Secret { get; set; }
}
