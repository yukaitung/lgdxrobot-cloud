namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public record MapDto
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public string Image { get; set; } = null!;
  public double Resolution { get; set; }
  public double OriginX { get; set; }
  public double OriginY { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}