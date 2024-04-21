using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Shared;
using LGDXRobot2Cloud.UI.Components.Progresses;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Progresses
  {
    private int? ProgressId { get; set; } = null;
    private ProgressTable? ProgressTable { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      ProgressId = null;
      ModalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      await ProgressTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      ProgressId = id;
    }
  }
}