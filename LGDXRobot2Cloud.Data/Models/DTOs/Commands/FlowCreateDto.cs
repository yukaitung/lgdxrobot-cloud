using LGDXRobot2Cloud.Data.Models.DTOs.Base;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class FlowCreateDto : FlowBaseDto
{
  public IEnumerable<FlowDetailCreateDto> FlowDetails { get; set; } = [];
}
