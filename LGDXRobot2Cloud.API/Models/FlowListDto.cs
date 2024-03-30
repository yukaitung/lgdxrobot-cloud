namespace LGDXRobot2Cloud.API.Models
{
  public class FlowListDto
  {
    public int Id { get; set; }
    
    public required string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}