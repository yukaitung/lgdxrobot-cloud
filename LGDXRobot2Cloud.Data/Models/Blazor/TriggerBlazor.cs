using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Data.Models.Blazor;

public class TriggerBlazor : IValidatableObject
{
  public int Id { get; set; }

  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;
  [Required]
  [MaxLength(200)]
  public string Url { get; set; } = null!;
  public string? Body { get; set; }

  public bool ApiKeyRequired { get; set; } = false;
  public int? ApiKeyInsertLocationId { get; set; } = (int)ApiKeyInsertLocation.Header;

  [MaxLength(50)]
  public string? ApiKeyFieldName { get; set; }
  public ApiKeyBlazor? ApiKey { get; set; }
  public int? ApiKeyId { get; set; }
  public string? ApiKeyName { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (ApiKeyRequired)
    {
      
      if (string.IsNullOrEmpty(ApiKeyFieldName))
      {
        switch (ApiKeyInsertLocationId)
        {
          case (int)ApiKeyInsertLocation.Body:
          case (int)ApiKeyInsertLocation.Query:
            yield return new ValidationResult("The key name is missing.", ["ApiKeyFieldName"]);
            break;
          case (int)ApiKeyInsertLocation.Header:
            yield return new ValidationResult("The header name is missing.", ["ApiKeyFieldName"]);
            break;
        }
      }
      if (ApiKeyId == null)
      {
        yield return new ValidationResult("The API Key has not been selected.", ["ApiKeyId"]);
      }
    }
  }
}
