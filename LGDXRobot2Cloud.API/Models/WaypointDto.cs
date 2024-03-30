namespace LGDXRobot2Cloud.API.Models
{
  public class WaypointDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}