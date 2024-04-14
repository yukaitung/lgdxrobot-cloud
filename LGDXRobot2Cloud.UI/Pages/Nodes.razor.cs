using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Nodes
  {
    private int? _nodeId { get; set; } = null;

    private void HandleItemSelect(int id)
    {
      _nodeId = id;
    }
  }
}