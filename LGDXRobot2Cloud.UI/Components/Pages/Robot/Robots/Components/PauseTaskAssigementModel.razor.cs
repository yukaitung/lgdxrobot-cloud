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
  public Models.Robot? Robot { get; set; }

  private bool IsError { get; set; } = false;

  public async Task HandleRequest()
  {
    bool newValue = !Robot!.IsPauseTaskAssigement;
    var success = await RobotService.UpdatePauseTaskAssigement(Robot!.Id.ToString(), newValue);
    if (success)
    {
      await JSRuntime.InvokeVoidAsync("CloseModal", "pauseTaskAssigement");
      Robot!.IsPauseTaskAssigement = newValue;
      Robot = null;
    } 
    else
      IsError = true;
  }
}