namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class FlowDetailDto
{
  public int Id { get; set; }
  public int Order { get; set; }
  public ProgressDto Progress { get; set; } = null!;
  public int AutoTaskNextControllerId { get; set; }
  public TriggerDto? StartTrigger { get; set; }
  public TriggerDto? EndTrigger { get; set; }
}
