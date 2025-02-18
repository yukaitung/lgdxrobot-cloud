using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Administration;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record ApiKeySecretUpdateDto
{
  [Required]
  [MaxLength(200)]
  public required string Secret { get; set; }
}

public static class ApiKeySecretUpdateDtoExtensions
{
  public static ApiKeySecretUpdateBusinessModel ToBusinessModel(this ApiKeySecretUpdateDto apiKeySecretUpdate)
  {
    return new ApiKeySecretUpdateBusinessModel
    {
      Secret = apiKeySecretUpdate.Secret
    };
  }
}