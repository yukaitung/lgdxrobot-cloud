using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskCreateDto : AutoTaskBaseDto
  {
    public IEnumerable<AutoTaskDetailCreateDto> Details { get; set; } = [];
    public bool IsTemplate { get; set; } = false;
  }
}