namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record ApiKeySecretDto
{
  public required string Secret { get; set; }
}
