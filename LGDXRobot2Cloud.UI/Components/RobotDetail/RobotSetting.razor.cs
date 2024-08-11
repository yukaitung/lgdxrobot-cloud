using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail;

public partial class RobotSetting
{
  [Parameter]
  public RobotBlazor? Robot { get; set; }
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private int CurrentTab { get; set; } = 0;
  private readonly List<string> Tabs = ["Robot Information", "System Information", "Chassis Information", "Robot Certificate"];

  private void HandleTabChange(int index)
  {
    CurrentTab = index;
  }

  protected async Task HandleValidSubmit()
  {

  }

  protected void HandleInvalidSubmit()
  {
  }

  protected override void OnInitialized()
  {
    Robot = new RobotBlazor();
    _editContext = new EditContext(Robot);
    _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
  }
}