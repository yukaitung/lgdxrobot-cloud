namespace LGDXRobot2Cloud.API.Models
{
  public class WaypointCreateDto
  {
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
  }
}