using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Administration;

public class ApiKeyDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string? Secret { get; set; } = null!;

  public bool IsThirdParty { get; set; }

  public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (IsThirdParty && string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("Secret is required for Third-Party API Key.", [nameof(Secret)]);
    }
    if (!IsThirdParty && !string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("LGDXRobot2 API Keys will be generated automatically.", [nameof(Secret)]);
    }
  }
}

public static class ApiKeyDetailViewModelExtensions
{
  public static void FromDto(this ApiKeyDetailViewModel apiKeyDetailViewModel, ApiKeyDto apiKeyDto)
  {
    apiKeyDetailViewModel.Id = (int)apiKeyDto.Id!;
    apiKeyDetailViewModel.Name = apiKeyDto.Name!;
    apiKeyDetailViewModel.IsThirdParty = (bool)apiKeyDto.IsThirdParty!;
  }

  public static ApiKeyUpdateDto ToUpdateDto(this ApiKeyDetailViewModel apiKeyDetailViewModel)
  {
    return new ApiKeyUpdateDto {
      Name = apiKeyDetailViewModel.Name,
    };
  }

  public static ApiKeyCreateDto ToCreateDto(this ApiKeyDetailViewModel apiKeyDetailViewModel)
  {
    return new ApiKeyCreateDto {
      Name = apiKeyDetailViewModel.Name,
      IsThirdParty = apiKeyDetailViewModel.IsThirdParty,
      Secret = apiKeyDetailViewModel.IsThirdParty ? apiKeyDetailViewModel.Secret : string.Empty
    };
  }
}