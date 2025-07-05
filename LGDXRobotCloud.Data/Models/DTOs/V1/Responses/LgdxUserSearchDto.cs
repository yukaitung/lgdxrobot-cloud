namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record LgdxUserSearchDto
{
  public required Guid Id { get; set; } 

  public string? UserName { get; set; }
}