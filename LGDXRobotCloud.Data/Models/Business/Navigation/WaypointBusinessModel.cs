using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record WaypointBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }

  public required double X { get; set; }

  public required double Y { get; set; }

  public required double Rotation { get; set; }

  public required bool IsParking { get; set; }

  public required bool HasCharger { get; set; }

  public required bool IsReserved { get; set; }
}

public static class WaypointBusinessModelExtensions
{
  public static WaypointDto ToDto(this WaypointBusinessModel model)
  {
    return new WaypointDto {
      Id = model.Id,
      Name = model.Name,
      Realm = new RealmSearchDto {
        Id = model.RealmId,
        Name = model.RealmName,
      },
      X = model.X,
      Y = model.Y,
      Rotation = model.Rotation,
      IsParking = model.IsParking,
      HasCharger = model.HasCharger,
      IsReserved = model.IsReserved,
    };
  }
}