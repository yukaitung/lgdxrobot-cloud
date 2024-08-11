using LGDXRobot2Cloud.Shared.Models.Blazor;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail;

public partial class RobotDetailTable
{
  [Parameter]
  public RobotBlazor? Robot { get; set; } = new RobotBlazor();
}