namespace LGDXRobot2Cloud.Shared.Models
{
  public class FlowDetailDto
  {
    public int Id { get; set; }
    public int Order { get; set; }
    public ProgressDto Progress { get; set; } = null!;
    public string ProceedCondition { get; set; } = null!;
    public TriggerDto? StartTrigger { get; set; }
    public TriggerDto? EndTrigger { get; set; }
  }
}