using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowCreateDto
  {
    [Required]
    public required string Name { get; set; }
    public IEnumerable<FlowDetailCreateDto> FlowDetails { get; set; } = new List<FlowDetailCreateDto>();
  }
}