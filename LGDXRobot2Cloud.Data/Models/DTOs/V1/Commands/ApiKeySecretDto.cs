using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Data.Models.Business.Administration;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

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