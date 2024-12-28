namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public record RealmDto
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public string? Description { get; set; }

  public required string Image { get; set; }

  public required double Resolution { get; set; }

  public required double OriginX { get; set; }

  public required double OriginY { get; set; }

  public required double OriginRotation { get; set; }
}