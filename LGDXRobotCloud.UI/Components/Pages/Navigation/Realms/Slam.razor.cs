using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Realms;

public enum SlamMode
{
  Normal = 0,
  SetGoal = 1
}

public record SlamMapMetadata
{
  public double Resolution { get; set; }
  public double OriginX { get; set; }
  public double OriginY { get; set; }
  public double OriginRotation { get; set; }
}

public sealed partial class Slam : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; }

  [Inject]
  public required ISlamDataService SlamDataService { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private Timer? Timer = null;
  private DotNetObjectReference<Slam> ObjectReference = null!;
  private Guid SelectedRobotId { get; set; } = Guid.Empty;
  private RobotData? SelectedRobot { get; set; }
  private SlamStatus SlamStatus { get; set; } = SlamStatus.Idle;
  private SlamMode SlamMode { get; set; } = SlamMode.Normal;
  private SlamMapMetadata SlamMapMetadata { get; set; } = new SlamMapMetadata();

  private void TimerStart()
  {
    Timer?.Change(0, 200);
  }

  private void TimerStop()
  {
    Timer?.Change(Timeout.Infinite, Timeout.Infinite);
  }

  [JSInvokable("HandleRobotSelect")]
  public void HandleRobotSelect(string _) { }

  [JSInvokable("HandleSetGoalSuccess")]
  public async Task HandleSetGoalSuccess(double x, double y, double rotation)
  {
    SlamMode = SlamMode.Normal;
    await LgdxApiClient.Navigation.Realms[Id!.Value].Slam.SetGoal.PostAsync(new RobotDofDto
    {
      X = x,
      Y = y,
      Rotation = rotation
    });
  }

  public async Task StartSetGoal()
  {
    SlamMode = SlamMode.SetGoal;
    await JSRuntime.InvokeVoidAsync("SlamMapSetGoalStart");
  }

  public async Task StopSetGoal()
  {
    SlamMode = SlamMode.Normal;
    await JSRuntime.InvokeVoidAsync("SlamMapSetGoalStop");
  }

  public async Task AbortGoal()
  {
    await LgdxApiClient.Navigation.Realms[Id!.Value].Slam.AbortGoal.PostAsync();
  }

  public async Task RefreshMap()
  {
    await LgdxApiClient.Navigation.Realms[Id!.Value].Slam.RefreshMap.PostAsync();
  }

  public async Task SaveMap()
  {
    await LgdxApiClient.Navigation.Realms[Id!.Value].Slam.SaveMap.PostAsync();
  }

  public async Task AbortSlam()
  {
    await LgdxApiClient.Navigation.Realms[Id!.Value].Slam.Abort.PostAsync();
    NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index + $"/{Id}");
  }

  public async Task UpdateRealm()
  {
    await JSRuntime.InvokeVoidAsync("SlamUpdateMap");
  }

  [JSInvokable("UpdateRealmStage2")]
  public async Task UpdateRealmStage2(string mapData)
  {
    TimerStop();
    await LgdxApiClient.Navigation.Realms[Id!.Value].Slam.Complete.PostAsync(new()
    {
      Resolution = SlamMapMetadata.Resolution,
      OriginX = SlamMapMetadata.OriginX,
      OriginY = SlamMapMetadata.OriginY,
      OriginRotation = SlamMapMetadata.OriginRotation,
      Image = mapData
    });
    CachedRealmService.ClearCache(Id!.Value);
    NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index + $"/{Id}");
  }

  private async Task OnUpdateSlamMap(SlamData slamData)
  {
    try
    {
      SelectedRobotId = slamData.RobotId;
      SlamStatus = slamData.SlamStatus;

      if (slamData.MapData != null)
      {
        SlamMapMetadata.Resolution = slamData.MapData.Resolution;
        SlamMapMetadata.OriginX = slamData.MapData.Origin.X;
        SlamMapMetadata.OriginY = slamData.MapData.Origin.Y;
        SlamMapMetadata.OriginRotation = slamData.MapData.Origin.Rotation;
        await JSRuntime.InvokeVoidAsync("UpdateSlamMapSpecification", slamData.MapData.Resolution, slamData.MapData.Origin.X, slamData.MapData.Origin.Y, slamData.MapData.Origin.Rotation);
        await JSRuntime.InvokeVoidAsync("UpdateSlamMap", slamData.MapData.Width, slamData.MapData.Height, slamData.MapData.Data);
      }
    }
    catch (Exception)
    {
      // Ignore
    }
  }

  private async Task OnRobotDataUpdated(Guid robotId, RobotData robotData)
  {
    try
    {
      await JSRuntime.InvokeVoidAsync("MoveRobot", robotId, robotData.Position.X, robotData.Position.Y, robotData.Position.Rotation);
      SelectedRobot = robotData;
      // Update Plan
      List<double> plan = [];
      foreach (var waypoint in robotData.NavProgress.Plan)
      {
        plan.Add(waypoint.X);
        plan.Add(waypoint.Y);
      }
      await JSRuntime.InvokeVoidAsync("UpdateRobotPlan", plan);
      await InvokeAsync(StateHasChanged);
    }
    catch (Exception)
    {
      // Ignore
    }
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitNavigationMap", ObjectReference);
      if (Id != null)
      {
        Timer = new Timer(async (state) =>
        {
          TimerStop();
          if (SelectedRobotId == Guid.Empty)
          {
            // Wait for slam data
            var slamData = await SlamDataService.GetSlamDataAsync(Id.Value);
            if (slamData != null)
            {
              await OnUpdateSlamMap(slamData);
            }
          }
          else
          {
            // Slam data is ready
            var data = await SlamDataService.GetAllDataAsync(Id.Value, SelectedRobotId);
            if (data.Item1 != null)
            {
              await OnRobotDataUpdated(SelectedRobotId, data.Item1);
            }
            if (data.Item2 != null)
            {
              await OnUpdateSlamMap(data.Item2);
            }
          }
          TimerStart();
        }, null, Timeout.Infinite, Timeout.Infinite);
        TimerStart();
      }
    }
    await base.OnAfterRenderAsync(firstRender);
  }

  public void Dispose()
  {
    Timer?.Dispose();
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}