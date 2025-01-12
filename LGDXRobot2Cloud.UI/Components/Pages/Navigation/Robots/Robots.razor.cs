using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots;

public sealed partial class Robots : ComponentBase
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  private List<RobotListDto>? RobotsList { get; set; }
  private Dictionary<Guid, RobotDataContract?> RobotsData { get; set; } = [];
  private Dictionary<Guid, RobotCommandsContract?> RobotsCommands { get; set; } = [];
  private RobotCommandsContract? SelectedRobotCommands { get; set; }

  private PaginationHelper? PaginationHelper { get; set; }
  private int CurrentPage { get; set; } = 1;
  private int PageSize { get; set; } = 16;
  private string DataSearch { get; set; } = string.Empty;
  private string LastDataSearch { get; set; } = string.Empty;

  private void SetRobotsData(IEnumerable<RobotListDto>? robots)
  {
    RobotsData.Clear();
    if (robots == null)
    {
      return;
    }
    foreach(var robot in robots)
    {
      Guid robotId = (Guid)robot.Id!;
      var robotData = RobotDataService.GetRobotData(robotId);
      if (robotData != null)
      {
        RobotsData[robotId] = robotData;
      }
      else
      {
        RobotsData[robotId] = new RobotDataContract{
          RobotId = robotId
        };
      }
      var robotCommands = RobotDataService.GetRobotCommands(robotId);
      if (robotCommands != null)
      {
        RobotsCommands[robotId] = robotCommands;
      }
      else
      {
        RobotsCommands[robotId] = new RobotCommandsContract{
          RobotId = robotId
        };
      };
    }
  }

  public async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    var robots = await LgdxApiClient.Navigation.Robots.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new() {
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
    SetRobotsData(robots);
    RobotsList = robots;
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
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

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    var robots = await LgdxApiClient.Navigation.Robots.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new() {
        Name = DataSearch,
        PageNumber = pageNum,
        PageSize = PageSize
      };
    });
    SetRobotsData(robots);
    RobotsList = robots;
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && RobotsList?.Count == 1)
      CurrentPage--;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    var robots = await LgdxApiClient.Navigation.Robots.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new() {
        Name = DataSearch,
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    SetRobotsData(robots);
    RobotsList = robots;
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
    //StateHasChanged();
  }

  public void HandleRobotSelect(RobotListDto robot)
  {
    SelectedRobotCommands = RobotsCommands[(Guid)robot.Id!];
  }

  protected override async Task OnInitializedAsync()
  {
    await Refresh();
    await base.OnInitializedAsync();
  }
}