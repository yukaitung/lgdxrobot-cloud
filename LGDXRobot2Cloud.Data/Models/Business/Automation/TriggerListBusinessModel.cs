using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record TriggerListBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required string Url { get; set; }

  public required int HttpMethodId { get; set; }
}

public static class TriggerListBusinessModelExtensions
{
  public static TriggerListDto ToDto(this TriggerListBusinessModel triggerListBusinessModel)
  {
    return new TriggerListDto {
      Id = triggerListBusinessModel.Id,
      Name = triggerListBusinessModel.Name,
      Url = triggerListBusinessModel.Url,
      HttpMethodId = triggerListBusinessModel.HttpMethodId,
    };
  }

  public static IEnumerable<TriggerListDto> ToDto(this IEnumerable<TriggerListBusinessModel> triggerListBusinessModels)
  {
    return triggerListBusinessModels.Select(p => p.ToDto());
  }
}