using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Navigation;
using LGDXRobotCloud.UI.ViewModels.Shared;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Robots;
public sealed partial class RobotDetail : ComponentBase, IAsyncDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public string Id { get; set; } = string.Empty;

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  private DeleteEntryModalViewModel DeleteEntryModalViewModel { get; set; } = new();
  private IDictionary<string,string>? DeleteEntryErrors = null;
  private RobotDetailViewModel RobotDetailViewModel { get; set; } = new();
  private RobotCertificateDto? RobotCertificate { get; set; } = null!;
  private RobotSystemInfoDto? RobotSystemInfoDto { get; set; } = null!;
  private RobotChassisInfoViewModel RobotChassisInfoViewModel { get; set; } = new();
  private List<AutoTaskListDto>? AutoTasks { get; set; }
  private RobotData? RobotData { get; set; }

  private Timer? Timer = null;

  private Guid IdGuid { get; set; }
  private int RealmId { get; set; }
  private string RealmName { get; set; } = string.Empty;
  private int CurrentTab { get; set; } = 0;
  private readonly List<string> Tabs = ["Robot", "System", "Chassis", "Certificate", "Activity Logs", "Delete Robot"];

  public void HandleTabChange(int index)
  {
    CurrentTab = index;
  }

  public async Task HandleTestDelete()
  {
    DeleteEntryModalViewModel.Errors = null;
    try
    {
      await LgdxApiClient.Navigation.Robots[RobotDetailViewModel!.Id].TestDelete.PostAsync();
      DeleteEntryModalViewModel.IsReady = true;
    }
    catch (ApiException ex)
    {
      DeleteEntryModalViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    DeleteEntryErrors = null;
    try
    {
      await LgdxApiClient.Navigation.Robots[RobotDetailViewModel!.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Navigation.Robots.Index);
    }
    catch (ApiException ex)
    {
      DeleteEntryErrors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandlePauseTaskAssignment()
  {
    bool enabled = RobotData!.RobotStatus != RobotStatus.Paused;
    await LgdxApiClient.Navigation.Robots[RobotDetailViewModel!.Id].PauseTaskAssignment.PatchAsync(new() {
      Enable = enabled
    });
  }

  public async Task HandleEmergencyStop()
  {
    bool enabled = !RobotData!.CriticalStatus.SoftwareEmergencyStop;
    await LgdxApiClient.Navigation.Robots[RobotDetailViewModel!.Id].EmergencyStop.PatchAsync(new() {
      Enable = enabled
    });
  }

  private void TimerStart()
  {
    Timer?.Change(0, 500);
  }

  private void TimerStop()
  {
    Timer?.Change(Timeout.Infinite, Timeout.Infinite);
  }

  private async Task OnRobotDataUpdated()
  {
    TimerStop();
    Guid robotId = Guid.Parse(Id);
    var robotData = await RobotDataService.GetRobotDataFromListAsync(RealmId, [IdGuid]);
    if (robotData.TryGetValue(robotId, out var rd))
    {
      RobotData = rd;
      await InvokeAsync(StateHasChanged);
    }
    TimerStart();
  }

  private async void OnAutoTaskUpdated(AutoTaskUpdate autoTaskUpdate)
  {
    if (AutoTasks == null)
      return;
    if (autoTaskUpdate.AssignedRobotId.ToString() != Id)
      return;

    // Update for running auto tasks
    if (!LgdxHelper.AutoTaskStaticStates.Contains(autoTaskUpdate.CurrentProgressId))
    {
      if (AutoTasks.Select(x => x.Id).Any())
      {
        AutoTasks.RemoveAll(x => x.Id == autoTaskUpdate.Id);
      }
      AutoTasks.Add(ConvertHelper.ToAutoTaskListDto(autoTaskUpdate, RealmName));
    }
    else if (autoTaskUpdate.CurrentProgressId == (int)ProgressState.Completed || autoTaskUpdate.CurrentProgressId == (int)ProgressState.Aborted)
    {
      AutoTasks.RemoveAll(x => x.Id == autoTaskUpdate.Id);
    }
    else if (autoTaskUpdate.CurrentProgressId == (int)ProgressState.Waiting)
    {
      AutoTasks.Add(ConvertHelper.ToAutoTaskListDto(autoTaskUpdate, RealmName));
    }
    AutoTasks = AutoTasks.OrderByDescending(x => x.CurrentProgress?.Id)
      .OrderByDescending(x => x.Priority)
      .ThenBy(x => x.Id)
      .ToList();
    await InvokeAsync(StateHasChanged);
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    RealmId = settings.CurrentRealmId;
    RealmName = await CachedRealmService.GetRealmName(settings.CurrentRealmId);

    if (Guid.TryParse(Id, out Guid _id))
    {
      var robot = await LgdxApiClient.Navigation.Robots[_id].GetAsync();
      RobotDetailViewModel.FromDto(robot!);
      RobotCertificate = robot!.RobotCertificate;
      RobotSystemInfoDto = robot!.RobotSystemInfo;
      RobotChassisInfoViewModel.FromDto(robot!.RobotChassisInfo!);
      AutoTasks = robot.AssignedTasks;
      IdGuid = _id;
    }
    
    await RealTimeService.SubscribeToTaskUpdateQueueAsync(RealmId, OnAutoTaskUpdated);
    Timer = new Timer(async (state) =>
    {
      await OnRobotDataUpdated();
    }, null, Timeout.Infinite, Timeout.Infinite);
    TimerStart();
    
    await base.OnInitializedAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await RealTimeService.UnsubscribeToTaskUpdateQueueAsync(RealmId, OnAutoTaskUpdated);
    Timer?.Dispose();
    GC.SuppressFinalize(this);
  }
}