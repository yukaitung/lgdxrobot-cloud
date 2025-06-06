namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record ApiKeyDto
{
  public required int Id { get; set; }

  public required string Name { get; set; } = null!;

  public required bool IsThirdParty { get; set; }
}
