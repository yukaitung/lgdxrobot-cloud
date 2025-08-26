using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobotCloud.UI.Components.Pages.Home;

public partial class Robots : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  private Timer? Timer = null;
  private int RealmId { get; set; }
  private List<RobotListDto>? RobotsList { get; set; }
  private Dictionary<Guid, RobotData?> RobotsData { get; set; } = [];

  private PaginationHelper? PaginationHelper { get; set; }
  private int CurrentPage { get; set; } = 1;
  private int PageSize { get; set; } = 16;
  private string DataSearch { get; set; } = string.Empty;
  private string LastDataSearch { get; set; } = string.Empty;

  private void TimerStart(int delay = 500)
  {
    Timer?.Change(delay, Timeout.Infinite);
  }

  private void TimerStartLong()
  {
    Timer?.Change(3000, Timeout.Infinite);
  }

  private void TimerStop()
  {
    Timer?.Change(Timeout.Infinite, Timeout.Infinite);
  }

  public async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    var robots = await LgdxApiClient.Navigation.Robots.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new()
      {
        RealmId = RealmId,
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
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
    var robots = await LgdxApiClient.Navigation.Robots.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new()
      {
        RealmId = RealmId,
        Name = DataSearch,
        PageNumber = pageNum,
        PageSize = PageSize
      };
    });
    RobotsList = robots;
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && RobotsList?.Count == 1)
      CurrentPage--;

    TimerStop();
    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    var robots = await LgdxApiClient.Navigation.Robots.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new()
      {
        RealmId = RealmId,
        Name = DataSearch,
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    RobotsList = robots;
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
    TimerStart(0);
  }

  private async Task OnRobotDataUpdated()
  {
    if (RobotsList == null)
      return;

    TimerStop();
    List<Guid> robotIds = [.. RobotsList.Where(x => x.Id != null).Select(x => x.Id!.Value)];
    RobotsData = await RobotDataService.GetRobotDataFromListAsync(RealmId, robotIds);
    await InvokeAsync(StateHasChanged);
    if (RobotsList.Count > 0)
    {
      TimerStart();
    }
    else
    {
      TimerStartLong();
    }
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    RealmId = settings.CurrentRealmId;
    Timer = new Timer(async (state) =>
    {
      await OnRobotDataUpdated();
    }, null, Timeout.Infinite, Timeout.Infinite);
    await Refresh();
    await base.OnInitializedAsync();
  }

  public void Dispose()
  {
    Timer?.Dispose();
    GC.SuppressFinalize(this);
  }
}