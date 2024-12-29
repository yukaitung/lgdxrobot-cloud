using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public sealed class RobotDetailViewModel : FormViewModel
{
  public Guid Id { get; set; }

  [MaxLength(50)]
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "Please select a realm.")]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; }
  
  public bool IsRealtimeExchange { get; set; }

  public bool IsProtectingHardwareSerialNumber { get; set; }
}