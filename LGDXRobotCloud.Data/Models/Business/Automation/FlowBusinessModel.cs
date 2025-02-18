using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record FlowBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required IList<FlowDetailBusinessModel> FlowDetails { get; set; } = [];
}

public static class FlowBusinessModelExtensions
{
  public static FlowDto ToDto(this FlowBusinessModel flowBusinessModel)
  {
    return new FlowDto {
      Id = flowBusinessModel.Id,
      Name = flowBusinessModel.Name,
      FlowDetails = flowBusinessModel.FlowDetails.Select(fd => fd.ToDto()).ToList(),
    };
  }
}