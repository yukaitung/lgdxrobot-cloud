namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class FlowDetailBlazor
  {
    public int? Id { get; set; }

    public int Order { get; set; }

    public ProgressBlazor? Progress { get; set; }

    public int? ProgressId { get; set; }

    public string? ProgressName { get; set; }

    public string ProceedCondition { get; set; } = null!;
  
    public TriggerBlazor? StartTrigger { get; set; }

    public int? StartTriggerId { get; set; }

    public string? StartTriggerName { get; set; }

    public TriggerBlazor? EndTrigger { get; set; }

    public int? EndTriggerId { get; set; }

    public string? EndTriggerName { get; set; }
    
  }
}