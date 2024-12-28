using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Administration.Users;

public class UserDetailViewModel : FormViewModel
{
  public Guid Id { get; set; }

  public string Name { get; set; } = null!;

  public string UserName { get; set; } = null!;

  public string Email { get; set; } = null!;

  public string Password { get; set; } = null!;

  public List<string> Roles { get; set; } = [];

  public bool TwoFactorEnabled { get; set; }
  
  public int AccessFailedCount { get; set; }
}