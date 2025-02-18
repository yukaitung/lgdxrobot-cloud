namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record FlowDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required IList<FlowDetailDto> FlowDetails { get; set; } = [];
}
