using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowUpdateDto
  {
    [Required]
    public required string Name { get; set; }
    public IEnumerable<FlowDetailUpdateDto> FlowDetails { get; set; } = new List<FlowDetailUpdateDto>();
  }
}