using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class NodeDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}