using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowUpdateDto : FlowBaseDto
  {
    public IEnumerable<FlowDetailUpdateDto> FlowDetails { get; set; } = [];
  }
}