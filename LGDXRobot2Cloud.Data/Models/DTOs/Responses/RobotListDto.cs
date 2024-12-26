namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotListDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public bool IsRealtimeExchange { get; set; }
  public string? Address { get; set; }
  public string? Namespace { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
