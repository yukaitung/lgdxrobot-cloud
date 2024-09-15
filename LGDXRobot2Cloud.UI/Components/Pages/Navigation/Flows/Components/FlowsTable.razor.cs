using LGDXRobot2Cloud.UI.Components.Shared.Table;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Flows.Components;

public sealed partial class FlowsTable : AbstractTable
{
  [Inject]
  public required IFlowService FlowService { get; set; }

  [Parameter]
  public EventCallback<int> OnIdSelected { get; set; }

  private List<Flow>? FlowsList { get; set; }
  
  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;
    var data = await FlowService.GetFlowsAsync(DataSearch, 1, PageSize);
    FlowsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    var data = await FlowService.GetFlowsAsync(DataSearch, 1, PageSize);
    FlowsList = data.Item1?.ToList();
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
    var data = await FlowService.GetFlowsAsync(DataSearch, pageNum, PageSize);
    FlowsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && FlowsList?.Count == 1)
      CurrentPage--;
    var data = await FlowService.GetFlowsAsync(DataSearch, CurrentPage, PageSize);
    FlowsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }
}
