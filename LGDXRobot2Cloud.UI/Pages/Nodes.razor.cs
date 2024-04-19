using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Components.Nodes;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Nodes
  {
    private int? NodeId { get; set; } = null;
    private NodeTable? NodeTable { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      NodeId = null;
      ModalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      await NodeTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      NodeId = id;
    }
  }
}