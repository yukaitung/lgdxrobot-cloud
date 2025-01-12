using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots;
public sealed partial class RobotDetail
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Parameter]
  public string Id { get; set; } = string.Empty;

  private RobotDetailViewModel RobotDetailViewModel { get; set; } = new();
  private RobotCertificateDto? RobotCertificate { get; set; } = null!;
  private RobotSystemInfoDto? RobotSystemInfoDto { get; set; } = null!;
  private RobotChassisInfoViewModel RobotChassisInfoViewModel { get; set; } = new();
  private IEnumerable<AutoTaskListDto>? AutoTasks { get; set; }
  private RobotDataContract? RobotData { get; set; }
  private RobotCommandsContract? RobotCommands { get; set; }

  private int CurrentTab { get; set; } = 0;
  private readonly List<string> Tabs = ["Robot", "System", "Chassis", "Certificate", "Delete Robot"];

  public void HandleTabChange(int index)
  {
    CurrentTab = index;
  }

  public async Task HandleDelete()
  {
    await LgdxApiClient.Navigation.Robots[RobotDetailViewModel!.Id].DeleteAsync();
    NavigationManager.NavigateTo(AppRoutes.Navigation.Robots.Index);
  }

  protected override async Task OnInitializedAsync()
  {
    if (Guid.TryParse(Id, out Guid _id))
    {
      var robot = await LgdxApiClient.Navigation.Robots[_id].GetAsync();
      RobotDetailViewModel.FromDto(robot!);
      RobotCertificate = robot!.RobotCertificate;
      RobotSystemInfoDto = robot!.RobotSystemInfo;
      RobotChassisInfoViewModel.FromDto(robot!.RobotChassisInfo!);
      AutoTasks = robot.AssignedTasks;
      RobotData = RobotDataService.GetRobotData(RobotDetailViewModel!.Id);
      RobotCommands = RobotDataService.GetRobotCommands(RobotDetailViewModel!.Id);
    }
    await base.OnInitializedAsync();
  }
}