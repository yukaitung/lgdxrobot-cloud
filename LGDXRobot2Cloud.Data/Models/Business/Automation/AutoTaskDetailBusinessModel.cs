using LGDXRobot2Cloud.Data.Models.Business.Navigation;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record AutoTaskDetailBusinessModel
{
  public required int Id { get; set; }

  public required int Order { get; set; }

  public double? CustomX { get; set; }

  public double? CustomY { get; set; }

  public double? CustomRotation { get; set; }
  
  public WaypointBusinessModel? Waypoint { get; set; }
}

public static class AutoTaskDetailBusinessModelExtensions
{
  public static AutoTaskDetailDto ToDto(this AutoTaskDetailBusinessModel model)
  {
    return new AutoTaskDetailDto {
      Id = model.Id,
      Order = model.Order,
      CustomX = model.CustomX,
      CustomY = model.CustomY,
      CustomRotation = model.CustomRotation,
      Waypoint = model.Waypoint == null ? null : new WaypointDto {
        Id = model.Waypoint.Id,
        Name = model.Waypoint.Name,
        Realm = new RealmSearchDto {
          Id = model.Waypoint.RealmId,
          Name = model.Waypoint.RealmName,
        },
        X = model.Waypoint.X,
        Y = model.Waypoint.Y,
        Rotation = model.Waypoint.Rotation,
        IsParking = model.Waypoint.IsParking,
        HasCharger = model.Waypoint.HasCharger,
        IsReserved = model.Waypoint.IsReserved,
      },
    };
  }
}