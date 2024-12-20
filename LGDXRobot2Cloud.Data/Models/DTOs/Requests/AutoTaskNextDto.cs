namespace LGDXRobot2Cloud.Data.Models.DTOs.Requests;

public record AutoTaskNextDto
{
  public Guid RobotId { get; set; }
  public string NextToken { get; set; } = null!;
}