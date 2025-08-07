namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record RealmCreateBusinessModel
{
  public required string Name { get; set; }

  public string? Description { get; set; }

  public required bool HasWaypointsTrafficControl { get; set; }

  public string? Image { get; set; }

  public double Resolution { get; set; }

  public double OriginX { get; set; }

  public double OriginY { get; set; }

  public double OriginRotation { get; set; }
}