using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Automation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record TriggerUpdateDto : IValidatableObject
{
  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public required string Name { get; set; }

  [Required (ErrorMessage = "Please enter an URL.")]
  [MaxLength(200)]
  public required string Url { get; set; }

  public required int HttpMethodId { get; set; }

  public string? Body { get; set; }

  public required bool SkipOnFailure { get; set; } = false;
  
  public int? ApiKeyInsertLocationId { get; set; }

  [MaxLength(50)]
  public string? ApiKeyFieldName { get; set; }

  public int? ApiKeyId { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (!(ApiKeyInsertLocationId == null && string.IsNullOrWhiteSpace(ApiKeyFieldName) && ApiKeyId == null))
    {
      if (ApiKeyInsertLocationId == null)
      {
        yield return new ValidationResult("Please select an insert location.", [nameof(ApiKeyInsertLocationId)]);
      }
      if (ApiKeyFieldName == null)
      {
        yield return new ValidationResult("Please enter a field name.", [nameof(ApiKeyFieldName)]);
      }
      if (ApiKeyId == null)
      {
        yield return new ValidationResult("Please select an API Key.", [nameof(ApiKeyId)]);
      }
    }
  }
}

public static class TriggerUpdateDtoExtensions
{
  public static TriggerUpdateBusinessModel ToBusinessModel(this TriggerUpdateDto triggerUpdateDto)
  {
    return new TriggerUpdateBusinessModel {
      Name = triggerUpdateDto.Name,
      Url = triggerUpdateDto.Url,
      HttpMethodId = triggerUpdateDto.HttpMethodId,
      Body = triggerUpdateDto.Body,
      SkipOnFailure = triggerUpdateDto.SkipOnFailure,
      ApiKeyInsertLocationId = triggerUpdateDto.ApiKeyInsertLocationId,
      ApiKeyFieldName = triggerUpdateDto.ApiKeyFieldName,
      ApiKeyId = triggerUpdateDto.ApiKeyId,
    };
  }
}