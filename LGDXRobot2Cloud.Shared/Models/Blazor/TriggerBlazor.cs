using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
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
    public string? ApiKeyInsertAt { get; set; }
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
          if (ApiKeyInsertAt == "header")
            yield return new ValidationResult("The header name is missing.", ["ApiKeyFieldName"]);
          else
            yield return new ValidationResult("The key name is missing.", ["ApiKeyFieldName"]);
        }
        if (ApiKeyId == null)
        {
          yield return new ValidationResult("The API Key has not been selected.", ["ApiKeyId"]);
        }
      }
    }
  }
}