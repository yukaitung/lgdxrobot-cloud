using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record WaypointTrafficBusinessModel
{
  public required int Id { get; set; }

  public required int WaypointFromId { get; set; }

  public required int WaypointToId { get; set; }
}

public static class WaypointTrafficBusinessModelExtensions
{
  public static WaypointTrafficDto ToDto(this WaypointTrafficBusinessModel model)
  {
    return new WaypointTrafficDto
    {
      Id = model.Id,
      WaypointFromId = model.WaypointFromId,
      WaypointToId = model.WaypointToId,
    };
  }
}