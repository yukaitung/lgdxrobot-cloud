using LGDXRobot2Cloud.UI.Components.Waypoints;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Waypoints
  {
    private int? _waypointId { get; set; } = null;
    private WaypointTable? _waypointTable { get; set; }
    private ModalSubmitDone? _modalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      _waypointId = null;
      _modalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      await _waypointTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      _waypointId = id;
    }
  }
}