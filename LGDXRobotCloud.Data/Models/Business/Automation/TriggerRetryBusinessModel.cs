using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record TriggerRetryBusinessModel 
{
  public required int Id { get; set; }

  public required int TriggerId { get; set; }

  public required string TriggerName { get; set; }

  public required string TriggerUrl { get; set; }

  public required int TriggerHttpMethodId { get; set; }

  public required int AutoTaskId { get; set; }

  public string? AutoTaskName { get; set; }

  public required string Body { get; set; }

  public required DateTime CreatedAt { get; set; }
}

public static class TriggerRetryBusinessModelExtensions
{
  public static TriggerRetryDto ToDto(this TriggerRetryBusinessModel triggerRetryBusinessModel)
  {
    return new TriggerRetryDto {
      Id = triggerRetryBusinessModel.Id,
      Trigger = new TriggerListDto {
        Id = triggerRetryBusinessModel.TriggerId,
        Name = triggerRetryBusinessModel.TriggerName,
        Url = triggerRetryBusinessModel.TriggerUrl,
        HttpMethodId = triggerRetryBusinessModel.TriggerHttpMethodId
      },
      AutoTask = new AutoTaskSearchDto {
        Id = triggerRetryBusinessModel.AutoTaskId,
        Name = triggerRetryBusinessModel.AutoTaskName
      },
      Body = triggerRetryBusinessModel.Body,
      CreatedAt = triggerRetryBusinessModel.CreatedAt
    };
  }
}