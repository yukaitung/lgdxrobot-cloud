using LGDXRobot2Cloud.Data.Models.DTOs.Base;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class AutoTaskCreateDto : AutoTaskBaseDto
{
  public IEnumerable<AutoTaskDetailCreateDto> Details { get; set; } = [];
  public bool IsTemplate { get; set; } = false;
}
