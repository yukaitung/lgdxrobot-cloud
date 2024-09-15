using LGDXRobot2Cloud.Utilities.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Models = LGDXRobot2Cloud.UI.Models;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots;

public sealed partial class Robots : ComponentBase
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  private List<Models.Robot>? RobotsList { get; set; }
  private PaginationHelper? PaginationHelper { get; set; }
  private int CurrentPage { get; set; } = 1;
  private int PageSize { get; set; } = 16;
  private string DataSearch { get; set; } = string.Empty;
  private string LastDataSearch { get; set; } = string.Empty;
  private Models.Robot? SelectedRobot { get; set; }

  public async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    var data = await RobotService.GetRobotsAsync(DataSearch, 1, PageSize);
    RobotsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
    LastDataSearch = DataSearch;
  }

  public async Task HandleClearSearch()
  {
    if (DataSearch == string.Empty && LastDataSearch == string.Empty)
      return;
    DataSearch = string.Empty;
    await HandleSearch();
  }

  public async Task HandlePageChange(int pageNum)
  {
    if (pageNum == CurrentPage)
      return;
    CurrentPage = pageNum;
    if (pageNum > PaginationHelper?.PageCount || pageNum < 1)
      return;
    var data = await RobotService.GetRobotsAsync(DataSearch, pageNum, PageSize);
    RobotsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && RobotsList?.Count == 1)
      CurrentPage--;
    var data = await RobotService.GetRobotsAsync(DataSearch, CurrentPage, PageSize);
    RobotsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
    StateHasChanged();
  }

  public void HandleRobotSelect(Models.Robot robot)
  {
    SelectedRobot = robot;
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