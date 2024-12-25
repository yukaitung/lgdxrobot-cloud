using System.Text.Json;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots;
public sealed partial class RobotDetail
{
  [Parameter]
  public string Id { get; set; } = string.Empty;

  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  private Models.Robot? Robot { get; set; }
  private RobotDataContract? RobotData { get; set; }

  private int CurrentTab { get; set; } = 0;
  private readonly List<string> Tabs = ["Robot", "System", "Chassis", "Certificate", "Delete Robot"];

  public void HandleTabChange(int index)
  {
    CurrentTab = index;
  }

  public async Task HandleDelete()
  {
    var success = await RobotService.DeleteRobotAsync(Robot!.Id.ToString());
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Robot.Robots.Index);
  }

  protected override async Task OnInitializedAsync()
  {
    Robot = await RobotService.GetRobotAsync(Id);
    RobotData = RobotDataService.GetRobotData(Robot!.Id);
    await base.OnInitializedAsync();
  }
}