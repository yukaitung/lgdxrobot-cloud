using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record ProgressUpdateDto
{
  [Required]
  [MaxLength(50)]
  public required string Name { get; set; }
}
