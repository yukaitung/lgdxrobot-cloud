using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Components.Flows;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Flows
  {
    private int? FlowId { get; set; } = null;
    private FlowTable? FlowTable { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      FlowId = null;
      ModalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      await FlowTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      FlowId = id;
    }
  }
}