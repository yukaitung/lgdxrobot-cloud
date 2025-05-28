using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record MapEditorUpdateDto
{
  public IEnumerable<WaypointLinkUpdateDto> WaypointLinks { get; set; } = [];
}

public static class MapEditUpdateDtoExtensions
{
  public static MapEditorUpdateBusinessModel ToBusinessModel(this MapEditorUpdateDto model)
  {
    return new MapEditorUpdateBusinessModel
    {
      WaypointLinks = model.WaypointLinks.Select(w => w.ToBusinessModel()),
    };
  }
}