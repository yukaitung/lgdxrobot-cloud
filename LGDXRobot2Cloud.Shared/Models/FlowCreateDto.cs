using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowCreateDto : FlowBaseDto
  {
    public IEnumerable<FlowDetailCreateDto> FlowDetails { get; set; } = [];
  }
}