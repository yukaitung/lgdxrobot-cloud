using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using LGDXRobot2Cloud.UI.ViewModels.Shared;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots;
public sealed partial class RobotDetail : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

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
  private RobotChassisInfoDto? RobotChassisInfoDto { get; set; } = new();
  private RobotChassisInfoViewModel RobotChassisInfoViewModel { get; set; } = new();
  private List<AutoTaskListDto>? AutoTasks { get; set; }
  private RobotDataContract? RobotData { get; set; }
  private RobotCommandsContract? RobotCommands { get; set; }

  private string RealmName { get; set; } = string.Empty;
  private int CurrentTab { get; set; } = 0;
  private readonly List<string> Tabs = ["Robot", "System", "Chassis", "Certificate", "Delete Robot"];

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

  private async void OnRobotDataUpdated(object? sender, RobotUpdatEventArgs updatEventArgs)
  {
    var robotId = updatEventArgs.RobotId;
    if (Id != robotId.ToString())
      return;
    
    var robotData = RobotDataService.GetRobotData(robotId, updatEventArgs.RealmId);
    if (robotData != null)
    {
      RobotData = robotData;
      await InvokeAsync(StateHasChanged);
    }
  }

  private async void OnRobotCommandsUpdated(object? sender, RobotUpdatEventArgs updatEventArgs)
  {
    var robotId = updatEventArgs.RobotId;
    if (Id != robotId.ToString())
      return;

    var robotCommands = RobotDataService.GetRobotCommands(updatEventArgs.RobotId);
    if (robotCommands != null)
    {
      RobotCommands = robotCommands;
      await InvokeAsync(StateHasChanged);
    }
  }

  private AutoTaskListDto ToAutoTaskListDto(AutoTaskUpdateContract autoTaskUpdateContract)
  {
    return new AutoTaskListDto{
      Id = autoTaskUpdateContract.Id,
      Name = autoTaskUpdateContract.Name,
      Priority = autoTaskUpdateContract.Priority,
      Flow = new FlowSearchDto {
        Id = autoTaskUpdateContract.FlowId,
        Name = autoTaskUpdateContract.FlowName
      },
      Realm = new RealmSearchDto {
        Id = autoTaskUpdateContract.RealmId,
        Name = RealmName
      },
      AssignedRobot = new RobotSearchDto {
        Id = autoTaskUpdateContract.AssignedRobotId,
        Name = autoTaskUpdateContract.AssignedRobotName
      },
      CurrentProgress = new ProgressSearchDto {
        Id = autoTaskUpdateContract.CurrentProgressId,
        Name = autoTaskUpdateContract.CurrentProgressName
      }
    };
  }

  private async void OnAutoTaskUpdated(object? sender, AutoTaskUpdatEventArgs updatEventArgs)
  {
    if (AutoTasks == null)
      return;
    var autoTaskUpdateContract = updatEventArgs.AutoTaskUpdateContract;
    if (autoTaskUpdateContract.AssignedRobotId.ToString() != Id)
      return;

    // Update for running auto tasks
    if (!LgdxHelper.AutoTaskStaticStates.Contains(autoTaskUpdateContract.CurrentProgressId))
    {
      if (AutoTasks.Select(x => x.Id).Any())
      {
        AutoTasks.RemoveAll(x => x.Id == autoTaskUpdateContract.Id);
      }
      AutoTasks.Add(ToAutoTaskListDto(autoTaskUpdateContract));
    }
    else if (autoTaskUpdateContract.CurrentProgressId == (int)ProgressState.Completed || autoTaskUpdateContract.CurrentProgressId == (int)ProgressState.Aborted)
    {
      AutoTasks.RemoveAll(x => x.Id == autoTaskUpdateContract.Id);
    }
    else if (autoTaskUpdateContract.CurrentProgressId == (int)ProgressState.Waiting)
    {
      AutoTasks.Add(ToAutoTaskListDto(autoTaskUpdateContract));
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
    var realmId = settings.CurrentRealmId;
    RealmName = CachedRealmService.GetRealmName(settings.CurrentRealmId);

    if (Guid.TryParse(Id, out Guid _id))
    {
      var robot = await LgdxApiClient.Navigation.Robots[_id].GetAsync();
      RobotDetailViewModel.FromDto(robot!);
      RobotCertificate = robot!.RobotCertificate;
      RobotSystemInfoDto = robot!.RobotSystemInfo;
      RobotChassisInfoDto = robot!.RobotChassisInfo;
      RobotChassisInfoViewModel.FromDto(robot!.RobotChassisInfo!);
      AutoTasks = robot.AssignedTasks;
      RobotData = RobotDataService.GetRobotData(RobotDetailViewModel!.Id, realmId);
      RobotCommands = RobotDataService.GetRobotCommands(RobotDetailViewModel!.Id);
    }

    RealTimeService.RobotDataUpdated += OnRobotDataUpdated;
    RealTimeService.RobotCommandsUpdated += OnRobotCommandsUpdated;
    RealTimeService.AutoTaskUpdated += OnAutoTaskUpdated;
    
    await base.OnInitializedAsync();
  }

  public void Dispose()
  {
    RealTimeService.RobotDataUpdated -= OnRobotDataUpdated;
    RealTimeService.RobotCommandsUpdated -= OnRobotCommandsUpdated;
    RealTimeService.AutoTaskUpdated -= OnAutoTaskUpdated;
    GC.SuppressFinalize(this);
  }
}