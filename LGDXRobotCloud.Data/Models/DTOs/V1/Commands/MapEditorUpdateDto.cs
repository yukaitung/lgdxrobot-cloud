using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record MapEditorUpdateDto
{
  public IEnumerable<WaypointTrafficUpdateDto> WaypointTraffics { get; set; } = [];
}

public static class MapEditUpdateDtoExtensions
{
  public static MapEditorUpdateBusinessModel ToBusinessModel(this MapEditorUpdateDto model)
  {
    return new MapEditorUpdateBusinessModel
    {
      WaypointTraffics = model.WaypointTraffics.Select(x => x.ToBusinessModel())
    };
  }
}