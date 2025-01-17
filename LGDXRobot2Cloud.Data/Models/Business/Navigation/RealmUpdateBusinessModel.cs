namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record RealmUpdateBusinessModel
{
  public required string Name { get; set; }

  public string? Description { get; set; }

  public required string Image { get; set; }

  public required double Resolution { get; set; }

  public required double OriginX { get; set; }

  public required double OriginY { get; set; }

  public required double OriginRotation { get; set; }
}