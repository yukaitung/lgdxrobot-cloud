namespace LGDXRobot2Cloud.Data.Models.Business.Navigation;

public record WaypointUpdateBusinessModel
{
  public required string Name { get; set; }

  public required double X { get; set; }

  public required double Y { get; set; }

  public required double Rotation { get; set; }

  public required bool IsParking { get; set; }

  public required bool HasCharger { get; set; }

  public required bool IsReserved { get; set; }
}