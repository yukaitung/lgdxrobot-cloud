using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace LGDXRobotCloud.UI.Components.Pages.Home;

public partial class Map : ComponentBase, IAsyncDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }
  
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  private Timer? Timer = null;
  private DotNetObjectReference<Map> ObjectReference = null!;

  private RealmDto Realm { get; set; } = null!;
  private RobotData? SelectedRobot { get; set; }
  private string SelectedRobotName { get; set; } = string.Empty;
  private Guid SelectedRobotId { get; set; } = Guid.Empty;
  private Dictionary<Guid, RobotData> RobotsData { get; set; } = [];
  private AutoTaskListDto? CurrentTask { get; set; }

  private void TimerStart()
  {
    Timer?.Change(0, 500);
  }

  private void TimerStartLong()
  {
    Timer?.Change(0, 3000);
  }

  private void TimerStop()
  {
    Timer?.Change(Timeout.Infinite, Timeout.Infinite);
  }

  [JSInvokable("HandleRobotSelect")]
  public async Task HandleRobotSelect(string robotId)
  {
    Guid newRobotId = Guid.Parse(robotId);
    if (newRobotId != SelectedRobotId)
    {
      var response = await LgdxApiClient.Navigation.Robots.Search.GetAsync(x => x.QueryParameters = new()
      {
        RealmId = Realm.Id,
        RobotId = newRobotId
      });
      if (response?.Count > 0)
      {
        SelectedRobotName = response[0].Name!;
      }
      CurrentTask = await LgdxApiClient.Automation.AutoTasks.RobotCurrentTask[newRobotId].GetAsync();
    }
    SelectedRobot = RobotsData.TryGetValue(newRobotId, out RobotData? value) ? value : null;
    StateHasChanged();
    SelectedRobotId = newRobotId;
  }

  public void HandleRobotManageClick()
  {
    NavigationManager.NavigateTo(AppRoutes.Navigation.Robots.Index + $"/{SelectedRobotId}?ReturnUrl=/?tab=1");
  }

  public void HandleViewTaskClick(int taskId)
  {
    NavigationManager.NavigateTo(AppRoutes.Automation.AutoTasks.Index + $"/{taskId}?ReturnUrl=/?tab=1");
  }

  private async void OnAutoTaskUpdated(AutoTaskUpdate autoTaskUpdate)
  {
    if (CurrentTask == null)
      return;
    if (autoTaskUpdate.AssignedRobotId != SelectedRobotId)
      return;

    // Update for running auto tasks
    if (!LgdxHelper.AutoTaskStaticStates.Contains(autoTaskUpdate.CurrentProgressId))
    {
      CurrentTask.Id = autoTaskUpdate.Id;
      CurrentTask.Name = autoTaskUpdate.Name;
      CurrentTask.Priority = autoTaskUpdate.Priority;
      CurrentTask.Flow!.Id = autoTaskUpdate.FlowId;
      CurrentTask.Flow!.Name = autoTaskUpdate.FlowName;
      CurrentTask.CurrentProgress!.Id = autoTaskUpdate.CurrentProgressId;
      CurrentTask.CurrentProgress!.Name = autoTaskUpdate.CurrentProgressName;
    }
    else if (autoTaskUpdate.CurrentProgressId == (int)ProgressState.Completed || autoTaskUpdate.CurrentProgressId == (int)ProgressState.Aborted)
    {
      CurrentTask = null;
    }
    await InvokeAsync(StateHasChanged);
  }

  private async Task OnRobotDataUpdated()
  {
    TimerStop();
    var robotData = await RobotDataService.GetRobotDataFromRealmAsync(Realm.Id!.Value);
    if (robotData.Count > 0)
    {
      try
      {
        // Update map
        foreach (var data in robotData)
        {
          var position = data.Value.Position;
          await JSRuntime.InvokeVoidAsync("MoveRobot", data.Key, position.X, position.Y, position.Rotation);
        }

        // Update current robot
        if (SelectedRobot != null && robotData.TryGetValue(SelectedRobotId, out var rd))
        {
          SelectedRobot = rd;
          // Update Plan
          List<double> plan = [];
          foreach (var waypoint in rd.NavProgress.Plan)
          {
            plan.Add(waypoint.X);
            plan.Add(waypoint.Y);
          }
          await JSRuntime.InvokeVoidAsync("UpdateRobotPlan", plan);
        }

        // Remove offline robots
        List<Guid> oldRobotIds = [.. RobotsData.Keys];
        List<Guid> newRobotIds = [.. robotData.Keys];
        List<Guid> removeRobotIds = [.. oldRobotIds.Except(newRobotIds)];
        foreach (var robotId in removeRobotIds)
        {
          await JSRuntime.InvokeVoidAsync("RemoveRobot", robotId);
        }

        RobotsData = robotData;
        await InvokeAsync(StateHasChanged);
      }
      catch (Exception)
      {
        // Ignore
      }
      TimerStart();
    }
    else
    {
      TimerStartLong();
    }
  }

  protected override async Task OnInitializedAsync() 
  {
    // Get Realm
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    Realm = await CachedRealmService.GetCurrrentRealmAsync(settings.CurrentRealmId);

    await base.OnInitializedAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitNavigationMap", ObjectReference);
      await RealTimeService.SubscribeToTaskUpdateQueueAsync(Realm.Id!.Value, OnAutoTaskUpdated);
      Timer = new Timer(async (state) =>
      {
        await OnRobotDataUpdated();
      }, null, Timeout.Infinite, Timeout.Infinite);
      TimerStart();
    }
    await base.OnAfterRenderAsync(firstRender);
  }

  public async ValueTask DisposeAsync()
  {
    await RealTimeService.UnsubscribeToTaskUpdateQueueAsync(Realm.Id!.Value, OnAutoTaskUpdated);
    Timer?.Dispose();
    ObjectReference?.Dispose();
    GC.SuppressFinalize(this);
  }
}