using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record WaypointTrafficUpdateDto
{
  public required int WaypointFromId { get; set; }

  public required int WaypointToId { get; set; }
}

public static class WaypointTrafficUpdateDtoExtensions
{
  public static WaypointTrafficUpdateBusinessModel ToBusinessModel(this WaypointTrafficUpdateDto model)
  {
    return new WaypointTrafficUpdateBusinessModel
    {
      WaypointFromId = model.WaypointFromId,
      WaypointToId = model.WaypointToId,
    };
  }
}