namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class NodeDto
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string ProcessName { get; set; } = null!;
  public string? Arguments { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
