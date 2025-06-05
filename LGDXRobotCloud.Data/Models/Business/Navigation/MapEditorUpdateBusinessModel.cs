namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record MapEditorUpdateBusinessModel
{
  public IEnumerable<WaypointTrafficUpdateBusinessModel> WaypointTraffics { get; set; } = [];
}