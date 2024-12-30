using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Automation;

public class TriggerDetailViewModel : FormViewModel
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  [MaxLength(200)]
  public string Url { get; set; } = null!;

  public int HttpMethodId { get; set; }

  public string? Body { get; set; }

  public bool SkipOnFailure { get; set; }

  // API Keys
  public bool ApiKeyRequired { get; set; }
  
  public int? ApiKeyInsertLocationId { get; set; }

  [MaxLength(50)]
  public string? ApiKeyFieldName { get; set; }

  public int? ApiKeyId { get; set; }

  public string? ApiKeyName { get; set; }
}