using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.UI.Models;

public class Node
{
  public int Id { get; set; }

  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [MaxLength(50)]
  [Required]
  public string ProcessName { get; set; } = null!;

  [MaxLength(200)]
  public string? Arguments { get; set; }

  public DateTime CreatedAt { get; set; }
  
  public DateTime UpdatedAt { get; set; }
}