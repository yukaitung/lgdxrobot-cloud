using System.ComponentModel.DataAnnotations;
using LGDXRobot2Cloud.UI.ViewModels.Shared;

namespace LGDXRobot2Cloud.UI.ViewModels.Navigation;

public class WaypointDetailViewModel : FormViewModel
{
  public int Id { get; set; }

  [MaxLength(100)]
  [Required]
  public string Name { get; set; } = null!;

  [Required]
  public int? RealmId { get; set; } = null;

  public string? RealmName { get; set; }
  
  [Required]
  public double X { get; set; }

  [Required]
  public double Y { get; set; }

  [Required]
  public double Rotation { get; set; }

  public bool IsParking { get; set; }

  public bool HasCharger { get; set; }

  public bool IsReserved { get; set; }
}