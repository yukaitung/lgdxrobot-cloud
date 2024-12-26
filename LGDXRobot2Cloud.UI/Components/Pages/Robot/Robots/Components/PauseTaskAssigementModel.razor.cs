using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots.Components;

public sealed partial class PauseTaskAssigementModel
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
    bool newValue = !RobotCommands!.Commands.PauseTaskAssigement;
    var success = await RobotService.UpdatePauseTaskAssigementAsync(RobotCommands!.RobotId.ToString(), newValue);
    if (success)
    {
      await JSRuntime.InvokeVoidAsync("CloseModal", "pauseTaskAssigement");
      RobotCommands!.Commands.PauseTaskAssigement = newValue;
      RobotCommands = null;
    } 
    else
      IsError = true;
  }
}