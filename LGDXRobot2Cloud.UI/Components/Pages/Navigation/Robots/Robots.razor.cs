using LGDXRobot2Cloud.Utilities.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots;

public sealed partial class Robots : ComponentBase
{
  [Inject]
  public required IRobotService RobotService { get; set; }

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
    var response = await RobotService.GetRobotsAsync(DataSearch, 1, PageSize);
    var data = response.Data;
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
    var response = await RobotService.GetRobotsAsync(DataSearch, pageNum, PageSize);
    var data = response.Data;
    SetRobotsData(data.Item1);
    RobotsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
  }

  public async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && RobotsList?.Count == 1)
      CurrentPage--;
    var response = await RobotService.GetRobotsAsync(DataSearch, CurrentPage, PageSize);
    var data = response.Data;
    SetRobotsData(data.Item1);
    RobotsList = data.Item1?.ToList();
    PaginationHelper = data.Item2;
    StateHasChanged();
  }

  public void HandleRobotSelect(RobotListDto robot)
  {
    SelectedRobotCommands = RobotsCommands[robot.Id];
  }

  protected override async Task OnInitializedAsync()
  {
    await Refresh();
    await base.OnInitializedAsync();
  }
}