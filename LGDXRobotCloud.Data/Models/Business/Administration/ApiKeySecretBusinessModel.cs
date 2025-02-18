using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record ApiKeySecretBusinessModel
{
  public required string Secret { get; set; }
}

public static class ApiKeySecretBusinessModelExtensions
{
  public static ApiKeySecretDto ToDto(this ApiKeySecretBusinessModel apiKeySecretDto)
  {
    return new ApiKeySecretDto
    {
      Secret = apiKeySecretDto.Secret
    };
  }
}