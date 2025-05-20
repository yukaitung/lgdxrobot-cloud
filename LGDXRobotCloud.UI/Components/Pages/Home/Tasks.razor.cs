using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobotCloud.UI.Components.Pages.Home;

public record RobotStatusStatistics
{
  public int OnlineRobots { get; set; } = 0;
  public int IdleRobots { get; set; } = 0;
  public int RunningRobots { get; set; } = 0;
  public int RobotsWithError { get; set; } = 0;
  public decimal Utilisation { get; set; } = 0;
}

public sealed partial class Tasks : ComponentBase
{
  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  private int RealmId { get; set; }

  private RobotStatusStatistics RobotStatusStatistics { get; set; } = new();

  void GenerateRobotStatusStatistics()
  {
    RobotStatusStatistics = new();
    var onlineRobots = RobotDataService.GetOnlineRobots(RealmId);
    foreach (var robotId in onlineRobots)
    {
      var robotData = RobotDataService.GetRobotData(robotId, RealmId);
      if (robotData != null)
      {
        RobotStatusStatistics.OnlineRobots++;
        if (robotData.RobotStatus == RobotStatus.Idle || robotData.RobotStatus == RobotStatus.Paused || robotData.RobotStatus == RobotStatus.Charging)
        {
          RobotStatusStatistics.IdleRobots++;
        }
        else if (robotData.RobotStatus == RobotStatus.Running || robotData.RobotStatus == RobotStatus.Aborting)
        {
          RobotStatusStatistics.RunningRobots++;
        }
        else if (robotData.RobotStatus == RobotStatus.Stuck || robotData.RobotStatus == RobotStatus.Critical)
        {
          RobotStatusStatistics.RobotsWithError++;
        }
      }
    }
    if (RobotStatusStatistics.OnlineRobots > 0)
    {
      RobotStatusStatistics.Utilisation = decimal.Round(RobotStatusStatistics.RunningRobots / RobotStatusStatistics.OnlineRobots * 100, 2);
    }
  }

  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    RealmId = settings.CurrentRealmId;
    await base.OnInitializedAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      GenerateRobotStatusStatistics();
    }
    await base.OnAfterRenderAsync(firstRender);
  }
}