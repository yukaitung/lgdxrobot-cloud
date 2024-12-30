using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LGDXRobot2Cloud.Data.Contracts;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class SoftwareEmergencyStopModel
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public RobotCommandsContract? RobotCommands { get; set; }

  private IDictionary<string, string[]>? Errors { get; set; } = null;

  public async Task HandleRequest()
  {
    bool newValue = !RobotCommands!.Commands.SoftwareEmergencyStop;
    var response = await RobotService.SetSoftwareEmergencyStopAsync(RobotCommands!.RobotId.ToString(), newValue);
    if (response.IsSuccess)
    {
      await JSRuntime.InvokeVoidAsync("CloseModal", "softwareEmergencyStop");
      RobotCommands!.Commands.SoftwareEmergencyStop = newValue;
      RobotCommands = null;
    } 
    else
      Errors = response.Errors;
  }
}