namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodeDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ProcessName { get; set; }
    public string? Arguments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}