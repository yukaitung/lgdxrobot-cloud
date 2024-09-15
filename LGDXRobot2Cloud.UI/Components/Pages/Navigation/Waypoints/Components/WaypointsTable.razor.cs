using LGDXRobot2Cloud.UI.Components.Shared.Components.Table;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;


namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Waypoints.Components;

public sealed partial class WaypointsTable : AbstractTable
{
  [Inject]
  public required IWaypointService WaypointService { get; set; }


  private List<Waypoint>? WaypointsList { get; set; }
  
  protected override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;
    var data = await WaypointService.GetWaypointsAsync(DataSearch, 1, PageSize);
    WaypointsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  protected override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    var data = await WaypointService.GetWaypointsAsync(DataSearch, 1, PageSize);
    WaypointsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
    LastDataSearch = DataSearch;
  }

  protected override async Task HandleClearSearch()
  {
    if (DataSearch == string.Empty && LastDataSearch == string.Empty)
      return;
    DataSearch = string.Empty;
    await HandleSearch();
  }

  protected override async Task HandlePageChange(int pageNum)
  {
    if (pageNum == CurrentPage)
      return;
    CurrentPage = pageNum;
    if (pageNum > PaginationHelper?.PageCount || pageNum < 1)
      return;
    var data = await WaypointService.GetWaypointsAsync(DataSearch, pageNum, PageSize);
    WaypointsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && WaypointsList?.Count == 1)
      CurrentPage--;
    var data = await WaypointService.GetWaypointsAsync(DataSearch, CurrentPage, PageSize);
    WaypointsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }
}
