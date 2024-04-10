namespace LGDXRobot2Cloud.Shared.Models
{
  public class AutoTaskListDto
  {
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Priority { get; set; }
    public required ProgressDto CurrentProgress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}