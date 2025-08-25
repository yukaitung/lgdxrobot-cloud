using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static LGDXRobotCloud.UI.Client.Automation.AutoTasks.AutoTasksRequestBuilder;

namespace LGDXRobotCloud.UI.Components.Pages.Home.Components;

public sealed partial class RealtimeAutoTasksTable : IAsyncDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public string Title { get; set; } = null!;

  [Parameter]
  public bool RunningAutoTasks { get; set; } = false;

  private int TotalAutoTasks { get; set; }
  private int MaxPageSize { get; set; } = 50;
  private int RealmId { get; set; }
  private string RealmName { get; set; } = string.Empty;
  private List<AutoTaskListDto>? AutoTasks { get; set; }

  private async Task Refresh()
  {
    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        RealmId = RealmId,
        AutoTaskCatrgory = (RunningAutoTasks ? AutoTaskCatrgory.Running : AutoTaskCatrgory.Waiting).ToString(),
        PageSize = MaxPageSize
      };
    });
    var PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
    TotalAutoTasks = PaginationHelper?.ItemCount ?? 0;
    AutoTasks ??= [];
  }

  private async void OnAutoTaskUpdated(AutoTaskUpdate autoTaskUpdate)
  {
    if (AutoTasks == null)
      return;

    if (RunningAutoTasks)
    {
      // Handle Running Auto Tasks
      if (!LgdxHelper.AutoTaskStaticStates.Contains(autoTaskUpdate.CurrentProgressId))
      {
        // Handle Running Auto Tasks
        if (AutoTasks.Where(x => x.Id == autoTaskUpdate.Id).Any())
        {
          AutoTasks.RemoveAll(x => x.Id == autoTaskUpdate.Id);
        }
        else
        {
          TotalAutoTasks++;
        }
        AutoTasks.Add(ConvertHelper.ToAutoTaskListDto(autoTaskUpdate, RealmName));
      }
      else if (autoTaskUpdate.CurrentProgressId == (int)ProgressState.Completed || autoTaskUpdate.CurrentProgressId == (int)ProgressState.Aborted)
      {
        // Handle Completed/Aborted Auto Tasks
        AutoTasks.RemoveAll(x => x.Id == autoTaskUpdate.Id);
        TotalAutoTasks--;
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
      if (autoTaskUpdate.CurrentProgressId == (int)ProgressState.Waiting)
      {
        // Handle Waiting Auto Tasks
        AutoTasks.Add(ConvertHelper.ToAutoTaskListDto(autoTaskUpdate, RealmName));
        TotalAutoTasks++;
      }
      else
      {
        // Handle Running/Completed/Aborted Auto Tasks
        if (AutoTasks.Where(x => x.Id == autoTaskUpdate.Id).Any())
        {
          TotalAutoTasks--;
        }
        AutoTasks.RemoveAll(x => x.Id == autoTaskUpdate.Id);
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
    RealmName = await CachedRealmService.GetRealmName(settings.CurrentRealmId);
    await Refresh();
    await RealTimeService.SubscribeToTaskUpdateQueueAsync(RealmId, OnAutoTaskUpdated);
    await base.OnInitializedAsync();
  }

  public async ValueTask DisposeAsync()
  {
    await RealTimeService.UnsubscribeToTaskUpdateQueueAsync(RealmId, OnAutoTaskUpdated);
    GC.SuppressFinalize(this);
  }
}