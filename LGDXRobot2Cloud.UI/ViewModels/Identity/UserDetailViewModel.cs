using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Identity;

public sealed class UserDetailViewModel : FormViewModel
{
  public Guid Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public string UserName { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a email.")]
  [EmailAddress (ErrorMessage = "Please enter a valid email.")]
  public string Email { get; set; } = null!;

  public List<string> Roles { get; set; } = [];
}