using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Client;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class SoftwareEmergencyStopModel
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public RobotCommandsContract? RobotCommands { get; set; }

  public async Task HandleRequest()
  {
    bool newValue = !RobotCommands!.Commands.SoftwareEmergencyStop;
    await LgdxApiClient.Navigation.Robots[RobotCommands!.RobotId].EmergencyStop.PatchAsync(new() {
      Enable = newValue
    });
    await JSRuntime.InvokeVoidAsync("CloseModal", "softwareEmergencyStop");
    RobotCommands!.Commands.SoftwareEmergencyStop = newValue;
    RobotCommands = null;
  }
}