namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record AutoTaskSearchDto
{
  public required int Id { get; set; }

  public string? Name { get; set; }

}