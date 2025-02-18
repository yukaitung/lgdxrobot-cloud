using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record TriggerRetryListBusinessModel 
{
  public required int Id { get; set; }

  public required int TriggerId { get; set; }

  public required string TriggerName { get; set; }

  public required int AutoTaskId { get; set; }

  public string? AutoTaskName { get; set; }

  public required DateTime CreatedAt { get; set; }
}

public static class TriggerRetryListBusinessModelExtensions
{
  public static TriggerRetryListDto ToDto(this TriggerRetryListBusinessModel triggerRetryListBusinessModel)
  {
    return new TriggerRetryListDto {
      Id = triggerRetryListBusinessModel.Id,
      Trigger = new TriggerSearchDto {
        Id = triggerRetryListBusinessModel.TriggerId,
        Name = triggerRetryListBusinessModel.TriggerName
      },
      AutoTask = new AutoTaskSearchDto {
        Id = triggerRetryListBusinessModel.AutoTaskId,
        Name = triggerRetryListBusinessModel.AutoTaskName
      },
      CreatedAt = triggerRetryListBusinessModel.CreatedAt
    };
  }

  public static IEnumerable<TriggerRetryListDto> ToDto(this IEnumerable<TriggerRetryListBusinessModel> triggerRetryListBusinessModels)
  {
    return triggerRetryListBusinessModels.Select(triggerRetryListBusinessModel => triggerRetryListBusinessModel.ToDto());
  }
}