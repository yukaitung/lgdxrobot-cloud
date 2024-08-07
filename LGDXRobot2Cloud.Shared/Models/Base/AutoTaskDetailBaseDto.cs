using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskDetailBaseDto
  {
    public double? CustomX { get; set; }

    public double? CustomY { get; set; }

    public double? CustomRotation { get; set; }
    
    public int? WaypointId { get; set; }

    [Required]
    public int Order { get; set; }
  }
}