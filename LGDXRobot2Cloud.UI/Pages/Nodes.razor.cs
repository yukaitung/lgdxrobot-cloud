using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Nodes
  {
    private int? _nodeId { get; set; } = null;

    // Data Edit
    private bool _submitDone { get; set; } = false;
    private CrudOperation _submitCrud { get; set; }
    private int _submitDoneId { get; set; }
    private string _submitDoneItemName { get; set; } = string.Empty;


    private void HandleSubmitDoneOpen((int, string, CrudOperation) data)
    {
      _submitDone = true;
      _submitDoneId = data.Item1;
      _submitDoneItemName = data.Item2;
      _submitCrud = data.Item3;
      if(_submitCrud == CrudOperation.Delete)
        _nodeId = null;
    }

    private void HandleSubmitDoneClose(bool display)
    {
      _submitDone = display;
    }

    private void HandleItemSelect(int id)
    {
      _nodeId = id;
    }
  }
}