using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record FlowSearchBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}

public static class FlowSearchBusinessModelExtensions
{
  public static FlowSearchDto ToDto(this FlowSearchBusinessModel flowSearchBusinessModel)
  {
    return new FlowSearchDto {
      Id = flowSearchBusinessModel.Id,
      Name = flowSearchBusinessModel.Name,
    };
  }

  public static IEnumerable<FlowSearchDto> ToDto(this IEnumerable<FlowSearchBusinessModel> flowSearchBusinessModels)
  {
    return flowSearchBusinessModels.Select(f => f.ToDto());
  }
}