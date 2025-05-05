namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record TriggerRetryDto
{
  public required int Id { get; set; }

  public required TriggerListDto Trigger { get; set; }

  public required AutoTaskSearchDto AutoTask { get; set; }

  public required string Body { get; set; }

  public required int SameTriggerFailed { get; set; }

  public required DateTime CreatedAt { get; set; }
}