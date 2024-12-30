using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration;

public class ApiKeyDetailSectretViewModel : FormViewModel
{
  
  public string? Secret { get; set; } = null;

  [Required (ErrorMessage = "Secret is required for Third-Party API Key.")]
  [MaxLength(200)]
  public string UpdateSecret { get; set; } = null!;
}