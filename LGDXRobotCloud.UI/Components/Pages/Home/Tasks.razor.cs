using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Home;

public sealed partial class Tasks : ComponentBase, IDisposable
{
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  private DotNetObjectReference<Tasks> ObjectReference = null!;
  public AutoTaskStatisticsDto? AutoTaskStatisticsDto { get; set; }
  public int RealmId { get; set; }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    RealmId = settings.CurrentRealmId;
    try
    {
      AutoTaskStatisticsDto = await LgdxApiClient.Automation.AutoTasks.Statistics[RealmId].GetAsync();
    }
    catch (ApiException ex)
    {
      // Prevent crashing the app if the API is not available
      Console.WriteLine(ex.Message);
    }
    await base.OnInitializedAsync();
  }
  
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitDotNet", ObjectReference);
      await JSRuntime.InvokeVoidAsync("GenerateChartWaitingTasks");
      await JSRuntime.InvokeVoidAsync("GenerateChartRunningTasks");
      await JSRuntime.InvokeVoidAsync("GenerateChartCompletedTasks");
      await JSRuntime.InvokeVoidAsync("GenerateChartAbortedTasks");
      if (AutoTaskStatisticsDto != null)
      {
        await JSRuntime.InvokeVoidAsync("UpdateChartWaitingTasks", AutoTaskStatisticsDto.WaitingTasksTrend);
        await JSRuntime.InvokeVoidAsync("UpdateChartRunningTasks", AutoTaskStatisticsDto.RunningTasksTrend);
        await JSRuntime.InvokeVoidAsync("UpdateChartCompletedTasks", AutoTaskStatisticsDto.CompletedTasksTrend);
        await JSRuntime.InvokeVoidAsync("UpdateChartAbortedTasks", AutoTaskStatisticsDto.AbortedTasksTrend);
      }
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}