using System.ComponentModel.DataAnnotations;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Requests;

public record AutoTaskNextDto
{
  [Required]
  public required int TaskId { get; set; }

  [Required]
  public required Guid RobotId { get; set; }

  [Required]
  public required string NextToken { get; set; }
}