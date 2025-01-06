namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record TriggerRetryListDto
{
  public required int Id { get; set; }

  public required TriggerSearchDto Trigger { get; set; }

  public required AutoTaskSearchDto AutoTask { get; set; }

  public required DateTime CreatedAt { get; set; }
}