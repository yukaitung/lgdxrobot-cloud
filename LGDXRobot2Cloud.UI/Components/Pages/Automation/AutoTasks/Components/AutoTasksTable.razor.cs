using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Components.Shared.Table;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using static LGDXRobot2Cloud.UI.Client.Automation.AutoTasks.AutoTasksRequestBuilder;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.AutoTasks.Components;

public sealed partial class AutoTasksTable : AbstractTable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public string Title { get; set; } = null!;

  [Parameter]
  public ProgressState? ShowProgressId { get; set; } = null;

  [Parameter]
  public bool ShowRunningTasks { get; set; } = false;

  private List<AutoTaskListDto>? AutoTasks { get; set; }

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

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        ShowProgressId = (int?)ShowProgressId,
        ShowRunningTasks = ShowRunningTasks,
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        ShowProgressId = (int?)ShowProgressId,
        ShowRunningTasks = ShowRunningTasks,
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
    LastDataSearch = DataSearch;
    CurrentPage = 1;
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

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        ShowProgressId = (int?)ShowProgressId,
        ShowRunningTasks = ShowRunningTasks,
        Name = DataSearch,
        PageNumber = pageNum,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && AutoTasks?.Count == 1)
      CurrentPage--;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        ShowProgressId = (int?)ShowProgressId,
        ShowRunningTasks = ShowRunningTasks,
        Name = DataSearch,
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }
}