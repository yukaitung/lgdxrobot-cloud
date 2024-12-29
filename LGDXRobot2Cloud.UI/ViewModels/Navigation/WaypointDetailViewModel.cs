using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public class WaypointDetailViewModel : FormViewModel
{
  public int Id { get; set; }

  [MaxLength(100)]
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "A realm is required.")]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; }
  
  [Required (ErrorMessage = "Please enter a X coordinate.")]
  public double? X { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a Y coordinate.")]
  public double? Y { get; set; } = null!;

  [Required (ErrorMessage = "Please enter a rotation.")]
  public double? Rotation { get; set; } = null!;

  public bool IsParking { get; set; }

  public bool HasCharger { get; set; }

  public bool IsReserved { get; set; }
}