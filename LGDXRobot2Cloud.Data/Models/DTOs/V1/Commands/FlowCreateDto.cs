using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public record FlowCreateDto
{
  [Required]
  public required string Name { get; set; }

  public required IList<FlowDetailCreateDto> FlowDetails { get; set; } = [];
}
