using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record FlowDetailBusinessModel
{
  public required int Id { get; set; }

  public required int Order { get; set; }

  public required int ProgressId { get; set; }

  public required string ProgressName { get; set; }

  public required int AutoTaskNextControllerId { get; set; }

  public int? TriggerId { get; set; }

  public string? TriggerName { get; set; }
}

public static class FlowDetailBusinessModelExtensions
{
  public static FlowDetailDto ToDto(this FlowDetailBusinessModel flowDetailBusinessModel)
  {
    return new FlowDetailDto {
      Id = flowDetailBusinessModel.Id,
      Order = flowDetailBusinessModel.Order,
      Progress = new ProgressSearchDto {
        Id = flowDetailBusinessModel.ProgressId,
        Name = flowDetailBusinessModel.ProgressName,
      },
      AutoTaskNextControllerId = flowDetailBusinessModel.AutoTaskNextControllerId,
      Trigger = flowDetailBusinessModel.TriggerId == null ? null : new TriggerSearchDto {
        Id = flowDetailBusinessModel.TriggerId.Value,
        Name = flowDetailBusinessModel.TriggerName!,
      },
    };
  }
}