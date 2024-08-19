namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class ProgressDto
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public bool System { get; set; }
  public bool Reserved { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
