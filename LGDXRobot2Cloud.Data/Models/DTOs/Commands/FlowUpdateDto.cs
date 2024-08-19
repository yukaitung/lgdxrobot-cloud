using LGDXRobot2Cloud.Data.Models.DTOs.Base;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class FlowUpdateDto : FlowBaseDto
{
  public IEnumerable<FlowDetailUpdateDto> FlowDetails { get; set; } = [];
}
