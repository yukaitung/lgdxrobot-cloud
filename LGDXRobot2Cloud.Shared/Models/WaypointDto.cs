using LGDXRobot2Cloud.Shared.Models.Base;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class WaypointDto : WaypointBaseDto
  {
    public int Id { get; set; }
    public override string Name { get; set; } = null!;
    public override double X { get; set; }
    public override double Y { get; set; }
    public override double W { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}