namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record ApiKeySearchDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}