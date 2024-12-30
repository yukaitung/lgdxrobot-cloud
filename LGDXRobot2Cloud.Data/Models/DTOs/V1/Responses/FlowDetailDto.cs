namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record FlowDetailDto
{
  public required int Id { get; set; }
  
  public required int Order { get; set; }

  public required ProgressDto Progress { get; set; }

  public required int AutoTaskNextControllerId { get; set; }

  public TriggerDto? Trigger { get; set; }
}
