namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record MapEditorUpdateBusinessModel
{
  public IEnumerable<WaypointTrafficUpdateBusinessModel> TrafficsToAdd { get; set; } = [];
  public IEnumerable<WaypointTrafficUpdateBusinessModel> TrafficsToDelete { get; set; } = [];
}