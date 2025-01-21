using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public sealed partial class PauseTaskAssigementModel
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }
  
  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public RobotCommandsContract? RobotCommands { get; set; }

  public async Task HandleRequest()
  {
    bool newValue = !RobotCommands!.Commands.PauseTaskAssigement;
    await LgdxApiClient.Navigation.Robots[RobotCommands!.RobotId].PauseTaskAssigement.PatchAsync(new() {
      Enable = newValue
    });
    await JSRuntime.InvokeVoidAsync("CloseModal", "pauseTaskAssigement");
    //RobotCommands!.Commands.PauseTaskAssigement = newValue;
    //RobotCommands = null;
  }
}