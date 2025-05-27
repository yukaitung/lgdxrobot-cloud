using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record WaypointLinkBusinessModel
{
  public required int Id { get; set; }

  public required int WaypointFromId { get; set; }

  public required int WaypointToId { get; set; }
}

public static class WaypointLinkBusinessModelExtensions
{
  public static WaypointLinkDto ToDto(this WaypointLinkBusinessModel model)
  {
    return new WaypointLinkDto
    {
      Id = model.Id,
      WaypointFromId = model.WaypointFromId,
      WaypointToId = model.WaypointToId,
    };
  }
}