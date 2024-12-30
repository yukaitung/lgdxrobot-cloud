using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public class TriggerDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "Please enter an URL.")]
  [MaxLength(200)]
  public string Url { get; set; } = null!;

  public int HttpMethodId { get; set; } = 1;

  public string? Body { get; set; }

  public bool SkipOnFailure { get; set; } = false;

  // API Keys
  public bool ApiKeyRequired { get; set; } = false;
  
  public int ApiKeyInsertLocationId { get; set; } = 1;

  [MaxLength(50)]
  public string? ApiKeyFieldName { get; set; }

  public int? ApiKeyId { get; set; }

  public string? ApiKeyName { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (ApiKeyRequired)
    {
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