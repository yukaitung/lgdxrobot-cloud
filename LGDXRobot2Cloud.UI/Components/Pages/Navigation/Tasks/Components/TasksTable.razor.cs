using LGDXRobot2Cloud.UI.Components.Shared.Table;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Tasks.Components;

public sealed partial class TasksTable : AbstractTable
{
  [Inject]
  public required IAutoTaskService AutoTaskService { get; set; }

  [Parameter]
  public string Title { get; set; } = null!;

  [Parameter]
  public ProgressState? ShowProgressId { get; set; } = null;

  [Parameter]
  public bool ShowRunningTasks { get; set; } = false;

  private List<AutoTask>? TasksList { get; set; }

  public bool IsEditable()
  {
    return ShowProgressId == ProgressState.Template;
  }
  
  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;
    var data = await AutoTaskService.GetAutoTasksAsync(ShowProgressId, ShowRunningTasks, DataSearch, 1, PageSize);
    TasksList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    var data = await AutoTaskService.GetAutoTasksAsync(ShowProgressId, ShowRunningTasks, DataSearch, 1, PageSize);
    TasksList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
    LastDataSearch = DataSearch;
  }

  public override async Task HandleClearSearch()
  {
    if (DataSearch == string.Empty && LastDataSearch == string.Empty)
      return;
    DataSearch = string.Empty;
    await HandleSearch();
  }

  public override async Task HandlePageChange(int pageNum)
  {
    if (pageNum == CurrentPage)
      return;
    CurrentPage = pageNum;
    if (pageNum > PaginationHelper?.PageCount || pageNum < 1)
      return;
    var data = await AutoTaskService.GetAutoTasksAsync(ShowProgressId, ShowRunningTasks, DataSearch, pageNum, PageSize);
    TasksList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && TasksList?.Count == 1)
      CurrentPage--;
    var data = await AutoTaskService.GetAutoTasksAsync(ShowProgressId, ShowRunningTasks, DataSearch, CurrentPage, PageSize);
    TasksList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }
}