using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Models = LGDXRobot2Cloud.UI.Models;
using Microsoft.JSInterop;
using LGDXRobot2Cloud.Data.Contracts;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots.Components;

public sealed partial class SoftwareEmergencyStopModel
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public RobotCommandsContract? RobotCommands { get; set; }

  private bool IsError { get; set; } = false;

  public async Task HandleRequest()
  {
    bool newValue = !RobotCommands!.Commands.SoftwareEmergencyStop;
    var success = await RobotService.UpdateSoftwareEmergencyStopAsync(RobotCommands!.RobotId.ToString(), newValue);
    if (success)
    {
      await JSRuntime.InvokeVoidAsync("CloseModal", "softwareEmergencyStop");
      RobotCommands!.Commands.SoftwareEmergencyStop = newValue;
      RobotCommands = null;
    } 
    else
      IsError = true;
  }
}