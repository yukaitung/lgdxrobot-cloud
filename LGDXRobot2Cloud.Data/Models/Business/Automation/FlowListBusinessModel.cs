using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record FlowListBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}

public static class FlowListBusinessModelExtensions
{
  public static FlowListDto ToDto(this FlowListBusinessModel flowListBusinessModel)
  {
    return new FlowListDto {
      Id = flowListBusinessModel.Id,
      Name = flowListBusinessModel.Name,
    };
  }

  public static IEnumerable<FlowListDto> ToDto(this IEnumerable<FlowListBusinessModel> flowListBusinessModels)
  {
    return flowListBusinessModels.Select(f => f.ToDto());
  }
}