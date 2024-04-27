using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Components.Tasks;
using LGDXRobot2Cloud.UI.Shared;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class Tasks
  {
    private int? TaskId { get; set; } = null;
    private TaskTable? WaitingTaskTable { get; set; }
    private TaskTable? SavedTaskTable { get; set; }
    private ModalSubmitDone? ModalSubmitDone { get; set; }
    private int CurrentTab { get; set; } = 0;
    private readonly List<string> Tabs = ["Ongoing Tasks", "Completed Tasks", "Aborted Tasks", "Task Templates"];

    private void HandleTabChange(int index)
    {
      ModalSubmitDone!.Close();
      CurrentTab = index;
    }
    
    private async Task HandleSubmitDoneOpen((int, string?, CrudOperation) data)
    {
      TaskId = null;
      ModalSubmitDone!.Open(data.Item1, data.Item2,data.Item3);
      if (CurrentTab == 0)
        await WaitingTaskTable!.Refresh(data.Item3 == CrudOperation.Delete);
      else if (CurrentTab == 3)
        await SavedTaskTable!.Refresh(data.Item3 == CrudOperation.Delete);
    }

    private void HandleItemSelect(int id)
    {
      TaskId = id;
    }
  }
}