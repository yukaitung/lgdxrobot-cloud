using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class FlowEditDto
  {
    [Required]
    public required string Name { get; set; }
    public IEnumerable<FlowDetailEditDto> FlowDetails { get; set; } = new List<FlowDetailEditDto>();
  }
}