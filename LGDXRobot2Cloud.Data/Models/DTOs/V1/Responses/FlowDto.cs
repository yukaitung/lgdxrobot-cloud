namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record FlowDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required IEnumerable<FlowDetailDto> FlowDetails { get; set; } = [];
}
