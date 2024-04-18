using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Components.Secrets;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Secrets
  {
    private int? _secretId { get; set; } = null;
    private LgdxApiTable? _lgdxApiTable { get; set; }
    private ModalSubmitDone? _modalSubmitDone { get; set; }
    private int _currentTab { get; set; } = 0;
    private List<string> _tabs = ["LGDXRobot2 API Keys", "Third-Party API Keys", "Certificates"];
    private List<string> _entitiesName = ["LGDXRobot2 API Key", "Third-Party API Key", "Certificate"];

    private void HandleTabChange(int index)
    {
      _currentTab = index;
    }

    private async Task HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      _secretId = null;
      _modalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      switch (_currentTab)
      {
        case 0:
          await _lgdxApiTable!.Refresh(data.Item3 == CrudOperation.Delete);
          break;
        case 1:
          break;
        case 2:
          break;
      }
      
    }

    private void HandleItemSelect(int id)
    {
      _secretId = id;
    }
  }
}