using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LGDXRobotCloud.UI.Components.Pages.Home;

public sealed partial class Map : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }
  
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  private DotNetObjectReference<Map> ObjectReference = null!;
  private RealmDto Realm { get; set; } = null!;
  private RobotDataContract? SelectedRobot { get; set; }
  private string SelectedRobotName { get; set; } = string.Empty;
  private Guid LastSelectedRobotId { get; set; } = Guid.Empty;
  private Guid SelectedRobotId { get; set; } = Guid.Empty;
  private Dictionary<Guid, RobotDataContract> RobotsData { get; set; } = [];

  [JSInvokable("HandleRobotSelect")]
  public async Task HandleRobotSelect(string robotId)
  {
    SelectedRobotId = Guid.Parse(robotId);
    if (LastSelectedRobotId != SelectedRobotId)
    {
      var response = await LgdxApiClient.Navigation.Robots.Search.GetAsync(x => x.QueryParameters = new() {
        RealmId = Realm.Id,
        RobotId = SelectedRobotId
      });
      if (response?.Count > 0)
      {
        SelectedRobotName = response[0].Name!;
      }
    }
    SelectedRobot = RobotsData.TryGetValue(SelectedRobotId, out RobotDataContract? value) ? value : null;
    StateHasChanged();
    LastSelectedRobotId = SelectedRobotId;
  }

  public void HandleRobotManageClick()
  {
    NavigationManager.NavigateTo(AppRoutes.Navigation.Robots.Index + $"/{SelectedRobotId}?ReturnUrl=/?tab=1");
  }

  private async void OnRobotDataUpdated(object? sender, RobotUpdatEventArgs updatEventArgs)
  {
    var robotId = updatEventArgs.RobotId;
    var realmId = Realm.Id ?? 0;
    if (realmId != updatEventArgs.RealmId)
    {
      return;
    }

    var robotData = RobotDataService.GetRobotData(robotId, realmId);
    try
    {
      if (robotData != null)
      {

        if (!RobotsData.ContainsKey(robotId))
        {
          await JSRuntime.InvokeVoidAsync("AddRobot", robotId, robotData.Position.X, robotData.Position.Y, robotData.Position.Rotation);
        }
        else
        {
          await JSRuntime.InvokeVoidAsync("MoveRobot", robotId, robotData.Position.X, robotData.Position.Y, robotData.Position.Rotation);
        }
        RobotsData[robotId] = robotData;
        // Update offcanvas
        if (SelectedRobot != null && SelectedRobot.RobotId == robotId)
        {
          SelectedRobot = robotData;
        }
        await InvokeAsync(StateHasChanged);
      }
    }
    catch (TaskCanceledException)
    {
      Console.WriteLine("TaskCanceledException");
    }
  }

  protected override async Task OnInitializedAsync() 
  {
    // Get Realm
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    Realm = await CachedRealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);
    var realmId = Realm.Id ?? 0;

    // Set Online Robots
    var onlineRobots = RobotDataService.GetOnlineRobots(realmId!);
    foreach (var robotId in onlineRobots)
    {
      var robotData = RobotDataService.GetRobotData(robotId, realmId!);
      if (robotData != null)
      {
        RobotsData.Add(robotId, robotData);
      }
    }
    await base.OnInitializedAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      RealTimeService.RobotDataUpdated += OnRobotDataUpdated;
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitNavigationMap", ObjectReference);
      foreach (var (robotId, robotData) in RobotsData)
      {
        await JSRuntime.InvokeVoidAsync("AddRobot", robotId, robotData.Position.X, robotData.Position.Y, robotData.Position.Rotation);
      }
    }
    await base.OnAfterRenderAsync(firstRender);
  }

  public void Dispose()
  {
    RealTimeService.RobotDataUpdated -= OnRobotDataUpdated;
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}