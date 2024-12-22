using LGDXRobot2Cloud.UI.Components.Shared.Table;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Maps.Components;

public sealed partial class MapsTable : AbstractTable
{
  [Inject]
  public required IMapsService MapsService { get; set; }

  private List<Map>? MapsList { get; set; }
  
  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;
    var data = await MapsService.GetMapsAsync(DataSearch, 1, PageSize);
    MapsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    var data = await MapsService.GetMapsAsync(DataSearch, 1, PageSize);
    MapsList = data.Item1?.ToList();
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
    var data = await MapsService.GetMapsAsync(DataSearch, pageNum, PageSize);
    MapsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && MapsList?.Count == 1)
      CurrentPage--;
    var data = await MapsService.GetMapsAsync(DataSearch, CurrentPage, PageSize);
    MapsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }
}
