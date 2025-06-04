using LGDXRobotCloud.Data.Models.Business.Navigation;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record MapEditorUpdateDto
{
  public IEnumerable<WaypointTrafficUpdateDto> TrafficsToAdd { get; set; } = [];
  public IEnumerable<WaypointTrafficUpdateDto> TrafficsToDelete { get; set; } = [];
}

public static class MapEditUpdateDtoExtensions
{
  public static MapEditorUpdateBusinessModel ToBusinessModel(this MapEditorUpdateDto model)
  {
    return new MapEditorUpdateBusinessModel
    {
      TrafficsToAdd = model.TrafficsToAdd.Select(w => w.ToBusinessModel()),
      TrafficsToDelete = model.TrafficsToDelete.Select(w => w.ToBusinessModel()),
    };
  }
}