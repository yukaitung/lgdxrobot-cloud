namespace LGDXRobot2Cloud.API.Models
{
  public class FlowDetailDto
  {
    public int Id { get; set; }
    public required ProgressDto Progress { get; set; }
    public required string ProceedCondition { get; set; }
    public TriggerDto? StartTrigger { get; set; }
    public TriggerDto? EndTrigger { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}