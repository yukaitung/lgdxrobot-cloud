namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

public class WaypointDto
{
  public int Id { get; set; }

  public string Name { get; set; } = null!;

  RealmListDto Realm { get; set; } = null!;

  public double X { get; set; }

  public double Y { get; set; }

  public double Rotation { get; set; }

  public bool IsParking { get; set; }

  public bool HasCharger { get; set; }

  public bool IsReserved { get; set; }
}
