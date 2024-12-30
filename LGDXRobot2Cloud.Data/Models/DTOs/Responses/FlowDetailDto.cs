using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class FlowDetailDto
{
  public int Id { get; set; }
  public int Order { get; set; }
  public ProgressDto Progress { get; set; } = null!;
  public int AutoTaskNextControllerId { get; set; }
  public TriggerDto? Trigger { get; set; }
}
