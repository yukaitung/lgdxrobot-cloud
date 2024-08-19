using LGDXRobot2Cloud.Data.Models.DTOs.Base;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class AutoTaskUpdateDto : AutoTaskBaseDto
{
  public IEnumerable<AutoTaskDetailUpdateDto> Details { get; set; } = [];
}
