using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Administration;

public class ApiKeyDetailsSectretViewModel : FormViewModel
{
  
  public string? Secret { get; set; } = null;

  [Required (ErrorMessage = "Secret is required for Third-Party API Key.")]
  [MaxLength(200)]
  public string UpdateSecret { get; set; } = null!;
}