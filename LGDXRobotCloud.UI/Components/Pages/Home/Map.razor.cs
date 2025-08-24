using System.Text.Json;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;
using StackExchange.Redis;
using static StackExchange.Redis.RedisChannel;

namespace LGDXRobotCloud.UI.Components.Pages.Home;

public sealed partial class Map : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }
  
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IConnectionMultiplexer RedisConnection { get; set; }

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
  private ISubscriber? Subscriber = null!;
  private DotNetObjectReference<Map> ObjectReference = null!;

  private RealmDto Realm { get; set; } = null!;
  private RobotDataContract? SelectedRobot { get; set; }
  private string SelectedRobotName { get; set; } = string.Empty;
  private Guid SelectedRobotId { get; set; } = Guid.Empty;
  private Dictionary<Guid, RobotDataContract> RobotsData { get; set; } = [];
  private AutoTaskListDto? CurrentTask { get; set; }

  private void TimerStart()
  {
    Timer?.Change(0, 500);
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
    SelectedRobot = RobotsData.TryGetValue(newRobotId, out RobotDataContract? value) ? value : null;
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

  private async Task OnAutoTaskUpdated(AutoTaskUpdateContract autoTaskUpdateContract)
  {
    if (CurrentTask == null)
      return;
    if (autoTaskUpdateContract.AssignedRobotId != SelectedRobotId)
      return;

    // Update for running auto tasks
    if (!LgdxHelper.AutoTaskStaticStates.Contains(autoTaskUpdateContract.CurrentProgressId))
    {
      CurrentTask.Id = autoTaskUpdateContract.Id;
      CurrentTask.Name = autoTaskUpdateContract.Name;
      CurrentTask.Priority = autoTaskUpdateContract.Priority;
      CurrentTask.Flow!.Id = autoTaskUpdateContract.FlowId;
      CurrentTask.Flow!.Name = autoTaskUpdateContract.FlowName;
      CurrentTask.CurrentProgress!.Id = autoTaskUpdateContract.CurrentProgressId;
      CurrentTask.CurrentProgress!.Name = autoTaskUpdateContract.CurrentProgressName;
    }
    else if (autoTaskUpdateContract.CurrentProgressId == (int)ProgressState.Completed || autoTaskUpdateContract.CurrentProgressId == (int)ProgressState.Aborted)
    {
      CurrentTask = null;
    }
    await InvokeAsync(StateHasChanged);
  }

  private async Task OnRobotDataUpdated()
  {
    TimerStop();
    var robotData = await RobotDataService.GetRobotDataFromRealmAsync(Realm.Id!.Value);
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
      Subscriber = RedisConnection.GetSubscriber();
      await Subscriber.SubscribeAsync(new RedisChannel($"autoTaskUpdate:{Realm.Id}", PatternMode.Literal), async (channel, value) =>
      {
        var update = JsonSerializer.Deserialize<AutoTaskUpdateContract>(value!);
        await OnAutoTaskUpdated(update!);
      });
      Timer = new Timer(async (state) =>
      {
        await OnRobotDataUpdated();
      }, null, Timeout.Infinite, Timeout.Infinite);
      TimerStart();
    }
    await base.OnAfterRenderAsync(firstRender);
  }

  public void Dispose()
  {
    Subscriber?.UnsubscribeAllAsync();
    Timer?.Dispose();
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}