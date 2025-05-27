using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record MapEditUpdateDto
{
  public IEnumerable<WaypointLinkUpdateDto> WaypointLinks { get; set; } = [];
}

public static class MapEditUpdateDtoExtensions
{
  public static MapEditUpdateBusinessModel ToBusinessModel(this MapEditUpdateDto model)
  {
    return new MapEditUpdateBusinessModel
    {
      WaypointLinks = model.WaypointLinks.Select(w => w.ToBusinessModel()),
    };
  }
}