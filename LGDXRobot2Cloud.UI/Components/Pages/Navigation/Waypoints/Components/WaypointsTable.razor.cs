using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Components.Shared.Table;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;


namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Waypoints.Components;

public sealed partial class WaypointsTable : AbstractTable
{
  [Inject]
  public required IWaypointService WaypointService { get; set; }

  private List<WaypointListDto>? Waypoints { get; set; }
  
  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;
    var data = await WaypointService.GetWaypointsAsync(DataSearch, 1, PageSize);
    Waypoints = data.Data.Item1?.ToList();
    PaginationHelper = data.Data.Item2;
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    var data = await WaypointService.GetWaypointsAsync(DataSearch, 1, PageSize);
    Waypoints = data.Data.Item1?.ToList();
    PaginationHelper = data.Data.Item2;
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
    var data = await WaypointService.GetWaypointsAsync(DataSearch, pageNum, PageSize);
    Waypoints = data.Data.Item1?.ToList();
    PaginationHelper = data.Data.Item2;
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && Waypoints?.Count == 1)
      CurrentPage--;
    var data = await WaypointService.GetWaypointsAsync(DataSearch, CurrentPage, PageSize);
    Waypoints = data.Data.Item1?.ToList();
    PaginationHelper = data.Data.Item2;
  }
}
