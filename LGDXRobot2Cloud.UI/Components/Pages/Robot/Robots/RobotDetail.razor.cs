using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots;
public partial class RobotDetail
{
  [Parameter]
  public string Id { get; set; } = string.Empty;

  [Inject]
  public required IRobotService RobotService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  private Models.Robot? Robot { get; set; }

  protected async Task HandleDelete()
  {
    var success = await RobotService.DeleteRobotAsync(Robot!.Id.ToString());
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Robot.Robots.Index);
  }

  public async Task Refresh()
  {
    Robot = await RobotService.GetRobotAsync(Id);
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      await Refresh();
      StateHasChanged();
    }
  }
}