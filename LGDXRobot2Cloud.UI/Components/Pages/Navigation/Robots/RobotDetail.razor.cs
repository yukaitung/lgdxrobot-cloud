using AutoMapper;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots;
public sealed partial class RobotDetail
{
  [Inject]
  public required IMapper Mapper { get; set; }

  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IRobotDataService RobotDataService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Parameter]
  public string Id { get; set; } = string.Empty;

  private RobotDetailViewModel? RobotDetailViewModel { get; set; } = null!;
  private RobotCertificateDto? RobotCertificate { get; set; } = null!;
  private RobotSystemInfoDto? RobotSystemInfoDto { get; set; } = null!;
  private RobotChassisInfoViewModel? RobotChassisInfoViewModel { get; set; } = null!;
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
    var response = await RobotService.DeleteRobotAsync(RobotDetailViewModel!.Id.ToString());
    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Robot.Robots.Index);
    else
      RobotDetailViewModel.Errors = response.Errors;
  }

  protected override async Task OnInitializedAsync()
  {
    var response = await RobotService.GetRobotAsync(Id);
    var data = response.Data;
    if (data != null)
    {
      RobotDetailViewModel = Mapper.Map<RobotDetailViewModel>(data);
      RobotCertificate = data.RobotCertificate;
      RobotSystemInfoDto = data.RobotSystemInfo;
      RobotChassisInfoViewModel = Mapper.Map<RobotChassisInfoViewModel>(data.RobotChassisInfo);
      AutoTasks = data.AssignedTasks;
    }
    RobotData = RobotDataService.GetRobotData(RobotDetailViewModel!.Id);
    RobotCommands = RobotDataService.GetRobotCommands(RobotDetailViewModel!.Id);
    await base.OnInitializedAsync();
  }
}