namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowDetailDto
  {
    public int Id { get; set; }
    public int Order { get; set; }
    public required ProgressDto Progress { get; set; }
    public required string ProceedCondition { get; set; }
    public TriggerDto? StartTrigger { get; set; }
    public TriggerDto? EndTrigger { get; set; }
  }
}