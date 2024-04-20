using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskCreateDto : AutoTaskBaseDto
  {
    public bool Saved { get; set; } = false;
  }
}