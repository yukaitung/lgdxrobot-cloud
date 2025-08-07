using LGDXRobotCloud.Data.Contracts;
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

public sealed partial class Slam : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required ISlamService SlamService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<Slam> ObjectReference = null!;
  private Guid SelectedRobotId { get; set; } = Guid.Empty;
  private RobotDataContract? SelectedRobot { get; set; }
  private SlamStatus SlamStatus { get; set; } = SlamStatus.Idle;
  private SlamMode SlamMode { get; set; } = SlamMode.Normal;

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
    SlamService.StopSlam(Id!.Value);
    NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index + $"/{Id}");
  }

  public async Task UpdateRealm()
  {
    await JSRuntime.InvokeVoidAsync("SlamUpdateMap");
  }

  [JSInvokable("UpdateRealmStage2")]
  public async Task UpdateRealmStage2(string mapData)
  {
    var slamData = SlamService.GetSlamData(Id!.Value);
    if (slamData == null)
    {
      return;
    }

    await LgdxApiClient.Navigation.Realms[Id!.Value].Slam.Complete.PostAsync(new()
    {
      Resolution = slamData.MapData!.Resolution,
      OriginX = slamData.MapData.Origin.X,
      OriginY = slamData.MapData.Origin.Y,
      OriginRotation = slamData.MapData.Origin.Rotation,
      Image = mapData
    });
    SlamService.StopSlam(Id!.Value);
    CachedRealmService.ClearCache(Id!.Value);
    NavigationManager.NavigateTo(AppRoutes.Navigation.Realms.Index + $"/{Id}");
  }


  private async Task UpdateSlamMap(int realmId)
  {
    var slamData = SlamService.GetSlamData(realmId);
    try
    {
      if (slamData != null)
      {
        SelectedRobotId = slamData.RobotId;
        SlamStatus = slamData.SlamStatus;

        if (slamData.MapData != null)
        {
          await JSRuntime.InvokeVoidAsync("UpdateSlamMapSpecification", slamData.MapData.Resolution, slamData.MapData.Origin.X, slamData.MapData.Origin.Y, slamData.MapData.Origin.Rotation);
          await JSRuntime.InvokeVoidAsync("UpdateSlamMap", slamData.MapData.Width, slamData.MapData.Height, slamData.MapData.Data);
        }
      }
    }
    catch (Exception)
    {
      Console.WriteLine("Exception");
    }
  }

  private async void OnSlamDataUpdated(object? sender, SlamDataUpdatEventArgs updatEventArgs)
  {
    if (updatEventArgs.RealmId != Id)
    {
      return;
    }
    await UpdateSlamMap(updatEventArgs.RealmId);
    await InvokeAsync(StateHasChanged);
  }

  private async void OnRobotDataUpdated(object? sender, RobotUpdatEventArgs updatEventArgs)
  {
    var robotId = updatEventArgs.RobotId;
    if (robotId != SelectedRobotId)
    {
      return;
    }

    var robotData = RobotDataService.GetRobotData(robotId, (int)Id!);
    try
    {
      if (robotData != null)
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
    }
    catch (Exception)
    {
      Console.WriteLine("Exception");
    }
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      SlamService.StartSlam(Id!.Value);
      RealTimeService.SlamDataUpdated += OnSlamDataUpdated;
      RealTimeService.RobotDataUpdated += OnRobotDataUpdated;
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitNavigationMap", ObjectReference);
      if (Id != null)
      {
        await UpdateSlamMap(Id.Value);
      }
    }
    await base.OnAfterRenderAsync(firstRender);
  }

  public void Dispose()
  {
    RealTimeService.SlamDataUpdated -= OnSlamDataUpdated;
    RealTimeService.RobotDataUpdated -= OnRobotDataUpdated;
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}