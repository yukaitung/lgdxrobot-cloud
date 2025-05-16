using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Robots;

public sealed partial class Robots : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  private int RealmId { get; set; }
  private string RealmName { get; set; } = string.Empty;
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
      var robotData = RobotDataService.GetRobotData(robotId, RealmId);
      if (robotData != null)
      {
        RobotsData[robotId] = robotData;
      }
      else
      {
        RobotsData[robotId] = new RobotDataContract{
          RobotId = robotId,
          RealmId = RealmId
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
          RobotId = robotId,
          RealmId = RealmId
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
        RealmId = RealmId,
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
    SetRobotsData(robots);
    RobotsList = robots;
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
    LastDataSearch = DataSearch;
    CurrentPage = 1;
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
        RealmId = RealmId,
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
        RealmId = RealmId,
        Name = DataSearch,
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    SetRobotsData(robots);
    RobotsList = robots;
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public void HandleRobotSelect(RobotListDto robot)
  {
    SelectedRobotCommands = RobotsCommands[(Guid)robot.Id!];
  }

  private async void OnRobotDataUpdated(object? sender, RobotUpdatEventArgs updatEventArgs)
  {
    var robotId = updatEventArgs.RobotId;
    if (updatEventArgs.RealmId != RealmId && !RobotsData.ContainsKey(robotId))
      return;
    
    var robotData = RobotDataService.GetRobotData(robotId, RealmId);
    if (robotData != null)
    {
      RobotsData[robotId] = robotData;
      await InvokeAsync(() => {
        StateHasChanged();
      });
    }
  }

  private async void OnRobotCommandsUpdated(object? sender, RobotUpdatEventArgs updatEventArgs)
  {
    var robotId = updatEventArgs.RobotId;
    if (updatEventArgs.RealmId != RealmId && !RobotsCommands.ContainsKey(robotId))
      return;

    var robotCommands = RobotDataService.GetRobotCommands(robotId);
    if (robotCommands != null)
    {
      RobotsCommands[robotId] = robotCommands;
      await InvokeAsync(() => {
        StateHasChanged();
      });
    }
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    RealmId = settings.CurrentRealmId;
    RealmName = await CachedRealmService.GetRealmName(settings.CurrentRealmId);
    await Refresh();
    RealTimeService.RobotDataUpdated += OnRobotDataUpdated;
    RealTimeService.RobotCommandsUpdated += OnRobotCommandsUpdated;
    await base.OnInitializedAsync();
  }

  public void Dispose()
  {
    RealTimeService.RobotDataUpdated -= OnRobotDataUpdated;
    RealTimeService.RobotCommandsUpdated -= OnRobotCommandsUpdated;
    GC.SuppressFinalize(this);
  }
}