using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.Blazor;

public class ProgressBlazor
{
  public int Id { get; set; }

  [MaxLength(50)]
  public string Name { get; set; } = null!;

  public bool System { get; set; } = false;

  public bool Reserved { get; set; } = false;

  public DateTime CreatedAt { get; set; }
  
  public DateTime UpdatedAt { get; set; }
}
