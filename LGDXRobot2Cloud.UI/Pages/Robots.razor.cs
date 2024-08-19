using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Shared;
using LGDXRobot2Cloud.UI.Components.Robots;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.Utilities.Services;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Robots
  {
    private RobotGrid? RobotGrid { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }

    private async Task HandleSubmitDoneOpen((Guid, string, CrudOperation) data)
    {
      ModalSubmitDone!.Open(data.Item1.ToString(), data.Item2, data.Item3);
      await RobotGrid!.Refresh(data.Item3 == CrudOperation.Delete);
    }
  }
}