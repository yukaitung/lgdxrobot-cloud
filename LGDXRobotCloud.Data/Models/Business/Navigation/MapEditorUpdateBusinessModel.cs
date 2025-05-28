namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record MapEditorUpdateBusinessModel
{
  public IEnumerable<WaypointLinkUpdateBusinessModel> WaypointLinks { get; set; } = [];
}