using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Components.Nodes;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Nodes
  {
    private int? _nodeId { get; set; } = null;
    private NodeTable? _nodeTable { get; set; }
    private ModalSubmitDone? _modalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      _nodeId = null;
      _modalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      await _nodeTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      _nodeId = id;
    }
  }
}