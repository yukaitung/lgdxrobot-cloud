using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Components.Secrets;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Secrets
  {
    private int? SecretId { get; set; } = null;
    private LgdxApiTable? LgdxApiTable { get; set; }
    private ThirdPartyApiTable? ThirdPartyApiTable { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }
    private int CurrentTab { get; set; } = 0;
    private readonly List<string> Tabs = ["LGDXRobot2 API Keys", "Third-Party API Keys", "Certificates"];
    private readonly List<string> EntitiesName = ["LGDXRobot2 API Key", "Third-Party API Key", "Certificate"];

    private void HandleTabChange(int index)
    {
      ModalSubmitDone!.Close();
      CurrentTab = index;
    }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      SecretId = null;
      ModalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      switch (CurrentTab)
      {
        case 0:
          await LgdxApiTable!.Refresh(data.Item3 == CrudOperation.Delete);
          break;
        case 1:
          await ThirdPartyApiTable!.Refresh(data.Item3 == CrudOperation.Delete);
          break;
        case 2:
          break;
      }
      
    }

    private void HandleItemSelect(int id)
    {
      SecretId = id;
    }
  }
}