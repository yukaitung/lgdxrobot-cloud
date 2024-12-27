using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration.Roles;

public sealed class RolesDetailViewModel : FormViewModel
{
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public List<string> Scopes { get; set; } = [];
}