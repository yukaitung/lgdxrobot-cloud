using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record FlowUpdateDto
{
  [Required]
  public required string Name { get; set; }
  
  public required IEnumerable<FlowDetailUpdateDto> FlowDetails { get; set; } = [];
}
