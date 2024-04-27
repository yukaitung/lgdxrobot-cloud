using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskCreateDto : AutoTaskBaseDto
  {
    public IEnumerable<AutoTaskDetailCreateDto> Waypoints { get; set; } = [];
    public bool Saved { get; set; } = false;
  }
}