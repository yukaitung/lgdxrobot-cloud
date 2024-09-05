using LGDXRobot2Cloud.Data.Models.Blazor;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Robots;

public partial class SoftwareEmergencyStop
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public RobotBlazor? Robot { get; set; }

  private bool IsError { get; set; } = false;

  protected async Task HandleRequest()
  {
    bool newValue = !Robot!.IsSoftwareEmergencyStop;
    var success = await RobotService.UpdateSoftwareEmergencyStop(Robot!.Id.ToString(), newValue);
    if (success)
    {
      await JSRuntime.InvokeVoidAsync("CloseModal", "softwareEmergencyStop");
      Robot!.IsSoftwareEmergencyStop = newValue;
      Robot = null;
    } 
    else
      IsError = true;
  }
}