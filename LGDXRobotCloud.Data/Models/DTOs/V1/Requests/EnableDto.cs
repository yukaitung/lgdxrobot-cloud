using System.ComponentModel.DataAnnotations;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Requests;

public record EnableDto
{
  [Required]
  public required bool Enable { get; set; }
}