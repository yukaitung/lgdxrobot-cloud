using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Components.Shared.Table;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;


namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Waypoints.Components;

public sealed partial class WaypointsTable : AbstractTable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  private List<WaypointListDto>? Waypoints { get; set; }
  
  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    Waypoints = await LgdxApiClient.Navigation.Waypoints.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new() {
        PageNumber = 1,
        PageSize = PageSize,
        Name = DataSearch
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    Waypoints = await LgdxApiClient.Navigation.Waypoints.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new() {
        PageNumber = 1,
        PageSize = PageSize,
        Name = DataSearch
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
    Waypoints = await LgdxApiClient.Navigation.Waypoints.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new() {
        PageNumber = pageNum,
        PageSize = PageSize,
        Name = DataSearch
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && Waypoints?.Count == 1)
      CurrentPage--;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    Waypoints = await LgdxApiClient.Navigation.Waypoints.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new() {
        PageNumber = CurrentPage,
        PageSize = PageSize,
        Name = DataSearch
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }
}
