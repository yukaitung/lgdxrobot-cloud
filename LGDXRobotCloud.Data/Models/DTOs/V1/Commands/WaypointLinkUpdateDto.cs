using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record WaypointLinkUpdateDto
{
  public int? Id { get; set; }

  public required int WaypointFromId { get; set; }

  public required int WaypointToId { get; set; }
}

public static class WaypointLinkUpdateDtoExtensions
{
  public static WaypointLinkUpdateBusinessModel ToBusinessModel(this WaypointLinkUpdateDto model)
  {
    return new WaypointLinkUpdateBusinessModel
    {
      Id = model.Id ?? 0,
      WaypointFromId = model.WaypointFromId,
      WaypointToId = model.WaypointToId,
    };
  }
}