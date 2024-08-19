namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class WaypointDto
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public double X { get; set; }
  public double Y { get; set; }
  public double Rotation { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
