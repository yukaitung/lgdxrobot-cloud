using LGDXRobot2Cloud.UI.Components.Waypoints;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Waypoints
  {
    private int? WaypointId { get; set; } = null;
    private WaypointTable? WaypointTable { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      WaypointId = null;
      ModalSubmitDone!.Open(data.Item1.ToString(), data.Item2,data.Item3);
      await WaypointTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      WaypointId = id;
    }
  }
}