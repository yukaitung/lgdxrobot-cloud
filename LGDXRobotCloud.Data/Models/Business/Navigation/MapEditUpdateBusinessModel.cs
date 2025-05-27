namespace LGDXRobotCloud.Data.Models.Business.Navigation;

public record MapEditUpdateBusinessModel
{
  public IEnumerable<WaypointLinkUpdateBusinessModel> WaypointLinks { get; set; } = [];
}