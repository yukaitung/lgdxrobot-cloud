namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record AutoTaskJourneyDto
{
  public required int Id { get; set; }

  public int? CurrentProcessId { get; set; }

  public string? CurrentProcessName { get; set; }

  public required DateTime CreatedAt { get; set; }
}