using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record WaypointListBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required int RealmId { get; set; }

  public required string RealmName { get; set; }

  public required double X { get; set; }

  public required double Y { get; set; }

  public required double Rotation { get; set; }
}

public static class WaypointListBusinessModelExtensions
{
  public static WaypointListDto ToDto(this WaypointListBusinessModel model)
  {
    return new WaypointListDto {
      Id = model.Id,
      Name = model.Name,
      Realm = new RealmSearchDto {
        Id = model.RealmId,
        Name = model.RealmName,
      },
      X = model.X,
      Y = model.Y,
      Rotation = model.Rotation,
    };
  }

  public static IEnumerable<WaypointListDto> ToDto(this IEnumerable<WaypointListBusinessModel> models)
  { 
    return models.Select(model => model.ToDto());
  }
}