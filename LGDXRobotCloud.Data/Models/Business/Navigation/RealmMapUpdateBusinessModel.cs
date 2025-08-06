namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record RealmMapUpdateBusinessModel
{
  public required string Image { get; set; }

  public required double Resolution { get; set; }

  public required double OriginX { get; set; }

  public required double OriginY { get; set; }

  public required double OriginRotation { get; set; }
}