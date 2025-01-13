using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record ApiKeySecretBusinessModel
{
  public required string Secret { get; set; }
}

public static class ApiKeySecretBusinessModelExtensions
{
  public static ApiKeySecretDto ToBusinessModel(this ApiKeySecretBusinessModel apiKeySecretDto)
  {
    return new ApiKeySecretDto
    {
      Secret = apiKeySecretDto.Secret
    };
  }
}