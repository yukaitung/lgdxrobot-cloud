using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Robots
{
  public partial class RobotGrid
  {
    [Inject]
    public required IRobotService RobotService { get; set; }

    private List<RobotBlazor>? RobotsList { get; set; }
    private PaginationMetadata? PaginationMetadata { get; set; }
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 16;
    private string DataSearch { get; set; } = string.Empty;
    private string LastDataSearch { get; set; } = string.Empty;

    protected async Task HandleSearch()
    {
      if (LastDataSearch == DataSearch)
        return;
      var data = await RobotService.GetRobotsAsync(DataSearch, 1, PageSize);
      RobotsList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
      LastDataSearch = DataSearch;
    }

    protected async Task HandleClearSearch()
    {
      if (DataSearch == string.Empty && LastDataSearch == string.Empty)
        return;
      DataSearch = string.Empty;
      await HandleSearch();
    }

    protected async Task HandlePageChange(int pageNum)
    {
      if (pageNum == CurrentPage)
        return;
      CurrentPage = pageNum;
      if (pageNum > PaginationMetadata?.PageCount || pageNum < 1)
        return;
      var data = await RobotService.GetRobotsAsync(DataSearch, pageNum, PageSize);
      RobotsList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    public async Task Refresh(bool deleteOpt = false)
    {
      if (deleteOpt && CurrentPage > 1 && RobotsList?.Count == 1)
        CurrentPage--;
      var data = await RobotService.GetRobotsAsync(DataSearch, CurrentPage, PageSize);
      RobotsList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
      StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        await Refresh();
        StateHasChanged();
      }
    }
  }
}