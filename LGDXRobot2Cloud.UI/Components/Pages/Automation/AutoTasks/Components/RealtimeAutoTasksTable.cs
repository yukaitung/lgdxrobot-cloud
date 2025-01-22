using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static LGDXRobot2Cloud.UI.Client.Automation.AutoTasks.AutoTasksRequestBuilder;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.AutoTasks.Components;

public sealed partial class RealtimeAutoTasksTable : IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public string Title { get; set; } = null!;

  [Parameter]
  public bool RunningAutoTasks { get; set; } = false;

  private int MaxPageSize { get; set; } = 50;
  private int RealmId { get; set; }
  private string RealmName { get; set; } = string.Empty;
  private List<AutoTaskListDto>? AutoTasks { get; set; }

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

  private async Task Refresh()
  {
    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        RealmId = RealmId,
        AutoTaskCatrgory = (int?)(RunningAutoTasks ? AutoTaskCatrgory.Running : AutoTaskCatrgory.Waiting),
        PageSize = MaxPageSize
      };
    });
    AutoTasks ??= [];
  }

  private async void OnAutoTaskUpdated(object? sender, AutoTaskUpdatEventArgs updatEventArgs)
  {
    if (AutoTasks == null)
      return;
    var autoTaskUpdateContract = updatEventArgs.AutoTaskUpdateContract;
    if (autoTaskUpdateContract.RealmId != RealmId)
      return;

    if (RunningAutoTasks)
    {
      // Handle Running Auto Tasks
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
      AutoTasks = AutoTasks!.OrderByDescending(x => x.Priority).ThenBy(x => x.Id).ToList();
      if (AutoTasks.Count >= MaxPageSize)
      {
        AutoTasks.RemoveAt(MaxPageSize);
      }
      await InvokeAsync(StateHasChanged);
    }
    else
    {
      // Handle Waiting Queue Auto Tasks
      if (autoTaskUpdateContract.CurrentProgressId == (int)ProgressState.Waiting)
      {
        AutoTasks.Add(ToAutoTaskListDto(autoTaskUpdateContract));
      }
      else
      {
        AutoTasks.RemoveAll(x => x.Id == autoTaskUpdateContract.Id);
      }
      AutoTasks = AutoTasks.OrderByDescending(x => x.Priority).ThenBy(x => x.Id).ToList();
      if (AutoTasks.Count >= MaxPageSize)
      {
        AutoTasks.RemoveAt(MaxPageSize);
      }
      await InvokeAsync(StateHasChanged);
    }
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    RealmId = settings.CurrentRealmId;
    RealmName = CachedRealmService.GetRealmName(settings.CurrentRealmId);
    RealTimeService.AutoTaskUpdated += OnAutoTaskUpdated;
    await Refresh();
    await base.OnInitializedAsync();
  }

  public void Dispose()
  {
    RealTimeService.AutoTaskUpdated -= OnAutoTaskUpdated;
    GC.SuppressFinalize(this);
  }
}