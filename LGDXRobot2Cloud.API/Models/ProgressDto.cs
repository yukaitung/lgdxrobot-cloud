namespace LGDXRobot2Cloud.API.Models
{
  public class ProgressDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool System { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}