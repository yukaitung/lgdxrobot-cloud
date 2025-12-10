using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Administration;

public class ApiKeyDetailsViewModel : FormViewModel, IValidatableObject
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
    if (Id == 0 && IsThirdParty && string.IsNullOrWhiteSpace(Secret))
    {
      yield return new ValidationResult("Secret is required for Third-Party API Key.", [nameof(Secret)]);
    }
  }
}

public static class ApiKeyDetailsViewModelExtensions
{
  public static void FromDto(this ApiKeyDetailsViewModel ApiKeyDetailsViewModel, ApiKeyDto apiKeyDto)
  {
    ApiKeyDetailsViewModel.Id = (int)apiKeyDto.Id!;
    ApiKeyDetailsViewModel.Name = apiKeyDto.Name!;
    ApiKeyDetailsViewModel.IsThirdParty = (bool)apiKeyDto.IsThirdParty!;
  }

  public static ApiKeyUpdateDto ToUpdateDto(this ApiKeyDetailsViewModel ApiKeyDetailsViewModel)
  {
    return new ApiKeyUpdateDto {
      Name = ApiKeyDetailsViewModel.Name,
    };
  }

  public static ApiKeyCreateDto ToCreateDto(this ApiKeyDetailsViewModel ApiKeyDetailsViewModel)
  {
    return new ApiKeyCreateDto {
      Name = ApiKeyDetailsViewModel.Name,
      IsThirdParty = ApiKeyDetailsViewModel.IsThirdParty,
      Secret = ApiKeyDetailsViewModel.IsThirdParty ? ApiKeyDetailsViewModel.Secret : string.Empty
    };
  }
}