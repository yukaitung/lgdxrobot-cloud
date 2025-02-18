using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record TriggerSearchBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}

public static class TriggerSearchBusinessModelExtensions
{
  public static TriggerSearchDto ToDto(this TriggerSearchBusinessModel triggerSearchBusinessModel)
  {
    return new TriggerSearchDto {
      Id = triggerSearchBusinessModel.Id,
      Name = triggerSearchBusinessModel.Name,
    };
  }

  public static IEnumerable<TriggerSearchDto> ToDto(this IEnumerable<TriggerSearchBusinessModel> triggerSearchBusinessModels)
  {
    return triggerSearchBusinessModels.Select(p => p.ToDto());
  }
}