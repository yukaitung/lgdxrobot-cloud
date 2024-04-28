using LGDXRobot2Cloud.UI.Components.NodesCollections;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class NodesCollections
  {
    private int? NodesCollectionId { get; set; } = null;
    private ModalSubmitDone? ModalSubmitDone { get; set; }
    private NodesCollectionTable? NodesCollectionTable { get; set; }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      NodesCollectionId = null;
      ModalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      await NodesCollectionTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      NodesCollectionId = id;
    }
  }
}