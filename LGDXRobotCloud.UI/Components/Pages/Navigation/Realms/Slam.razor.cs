using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Realms;
public sealed partial class Slam : ComponentBase, IDisposable
{
  [Inject]
  public required IRealTimeService RealTimeService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required ISlamService SlamService { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<Slam> ObjectReference = null!;
  private Guid SelectedRobotId { get; set; } = Guid.Empty;
  private RobotDataContract? SelectedRobot { get; set; }

  [JSInvokable("HandleRobotSelect")]
  public void HandleRobotSelect(string robotId)
  {
    Console.WriteLine($"Robot Selected: {robotId}");
  }

  public async Task SetGoal()
  {
    await JSRuntime.InvokeVoidAsync("SlamMapSetGoalStart");
  }

  private async Task UpdateSlamMap(int realmId)
  {
    var slamData = SlamService.GetSlamData(realmId);
    try
    {
      if (slamData != null)
      {
        SelectedRobotId = slamData.RobotId;

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

  private async void OnMapDataUpdated(object? sender, SlamMapDataUpdatEventArgs updatEventArgs)
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
      RealTimeService.SlamMapDataUpdated += OnMapDataUpdated;
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
    RealTimeService.SlamMapDataUpdated -= OnMapDataUpdated;
    RealTimeService.RobotDataUpdated -= OnRobotDataUpdated;
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}