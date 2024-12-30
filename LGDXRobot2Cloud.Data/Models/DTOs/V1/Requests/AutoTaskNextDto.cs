using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;

public record AutoTaskNextDto
{
  [Required]
  public required Guid RobotId { get; set; }

  public required string NextToken { get; set; }
}