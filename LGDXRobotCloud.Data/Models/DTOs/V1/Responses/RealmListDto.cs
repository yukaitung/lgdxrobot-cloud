namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record RealmListDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public string? Description { get; set; }

  public required double Resolution { get; set; }
}