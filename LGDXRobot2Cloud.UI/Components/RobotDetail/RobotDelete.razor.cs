using LGDXRobot2Cloud.Data.Models.Blazor;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail;

public partial class RobotDelete
{
  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required NavigationManager NavManager { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public RobotBlazor? Robot { get; set; }

  private bool IsError { get; set; } = false;

  protected async void HandleDelete()
  {
    var success = await RobotService.DeleteRobotAsync(Robot!.Id.ToString());
    if (success)
    {
      // DO NOT REVERSE THE ORDER
      await JSRuntime.InvokeVoidAsync("CloseModal", "robotDeleteModal");
      NavManager.NavigateTo("/robots/robots");
    } 
    else
      IsError = true;
  }
}
