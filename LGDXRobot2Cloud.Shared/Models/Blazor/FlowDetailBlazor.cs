namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class FlowDetailBlazor
  {
    public int Id { get; set; }

    public int Order { get; set; }

    public ProgressBlazor Progress { get; set; } = null!;

    public string ProceedCondition { get; set; } = null!;
  
    public TriggerBlazor? StartTrigger { get; set; }

    public TriggerBlazor? EndTrigger { get; set; }
  }
}