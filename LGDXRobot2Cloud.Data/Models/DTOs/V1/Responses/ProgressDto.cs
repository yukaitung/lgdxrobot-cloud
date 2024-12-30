namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record ProgressDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required bool System { get; set; }

  public required bool Reserved { get; set; }
}
