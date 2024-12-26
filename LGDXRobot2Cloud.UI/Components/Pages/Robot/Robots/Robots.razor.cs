using LGDXRobot2Cloud.Utilities.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Models = LGDXRobot2Cloud.UI.Models;
using Microsoft.AspNetCore.Components;
using LGDXRobot2Cloud.Data.Contracts;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots;

public sealed partial class Robots : ComponentBase
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  private List<Models.Robot>? RobotsList { get; set; }
  private Dictionary<Guid, RobotDataContract?> RobotsData { get; set; } = [];
  private Dictionary<Guid, RobotCommandsContract?> RobotsCommands { get; set; } = [];
  private PaginationHelper? PaginationHelper { get; set; }
  private int CurrentPage { get; set; } = 1;
  private int PageSize { get; set; } = 16;
  private string DataSearch { get; set; } = string.Empty;
  private string LastDataSearch { get; set; } = string.Empty;
  private RobotCommandsContract? SelectedRobotCommands { get; set; }

  private void SetRobotsData(IEnumerable<Models.Robot>? robots)
  {
    RobotsData.Clear();
    if (robots == null)
    {
      return;
    }
    foreach(var robot in robots)
    {
      var robotData = RobotDataService.GetRobotData(robot.Id);
      if (robotData != null)
      {
        RobotsData[robot.Id] = robotData;
      }
      else
      {
        RobotsData[robot.Id] = new RobotDataContract{
          RobotId = robot.Id
        };
      }
      var robotCommands = RobotDataService.GetRobotCommands(robot.Id);
      if (robotCommands != null)
      {
        RobotsCommands[robot.Id] = robotCommands;
      }
      else
      {
        RobotsCommands[robot.Id] = new RobotCommandsContract{
          RobotId = robot.Id
        };
      };
    }
  }

  public async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    var data = await RobotService.GetRobotsAsync(DataSearch, 1, PageSize);
    SetRobotsData(data.Item1);
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
    SetRobotsData(data.Item1);
    RobotsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && RobotsList?.Count == 1)
      CurrentPage--;
    var data = await RobotService.GetRobotsAsync(DataSearch, CurrentPage, PageSize);
    SetRobotsData(data.Item1);
    RobotsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
    StateHasChanged();
  }

  public void HandleRobotSelect(Models.Robot robot)
  {
    SelectedRobotCommands = RobotsCommands[robot.Id];
  }

  protected override async Task OnInitializedAsync()
  {
    await Refresh();
    await base.OnInitializedAsync();
  }
}