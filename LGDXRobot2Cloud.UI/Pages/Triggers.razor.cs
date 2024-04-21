using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Shared;
using LGDXRobot2Cloud.UI.Components.Triggers;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Triggers
  {
    private int? TriggerId { get; set; } = null;
    private TriggerTable? TriggerTable { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      TriggerId = null;
      ModalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      await TriggerTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      TriggerId = id;
    }
  }
}