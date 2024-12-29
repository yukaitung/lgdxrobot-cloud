namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record RobotSearchDto
{
  public required Guid Id { get; set; }

  public required string Name { get; set; }
}
