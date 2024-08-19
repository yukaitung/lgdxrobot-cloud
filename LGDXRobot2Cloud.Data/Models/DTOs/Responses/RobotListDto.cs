namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class RobotListDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string Address { get; set; } = null!;
  public int RobotStatusId { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
