using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Blazor;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Pages
{
  public partial class RobotDetail
  {
    [Parameter]
    public string RobotId { get; set; } = string.Empty;

    [Inject]
    public required IRobotService RobotService { get; set; }

    private RobotBlazor? Robot { get; set; }

    public async Task Refresh()
    {
      Robot = await RobotService.GetRobotAsync(RobotId);
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
}