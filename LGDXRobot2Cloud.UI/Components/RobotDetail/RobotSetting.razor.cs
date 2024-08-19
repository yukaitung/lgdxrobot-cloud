using LGDXRobot2Cloud.Data.Models.Blazor;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail;

public partial class RobotSetting
{
  [Parameter]
  public RobotBlazor? Robot { get; set; }

  [Parameter]
  public EventCallback OnUpdated { get; set; }
  
  private int CurrentTab { get; set; } = 0;
  private readonly List<string> Tabs = ["Robot Information", "System Information", "Chassis Information", "Robot Certificate", "Delete Robot"];

  private void HandleTabChange(int index)
  {
    CurrentTab = index;
  }

  private async void HandleUpdate()
  {
    await OnUpdated.InvokeAsync();
  }
}