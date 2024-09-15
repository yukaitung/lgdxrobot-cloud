using LGDXRobot2Cloud.UI.Models;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Robots.Components;

public partial class TasksTable
{
  [Parameter]
  public ICollection<AutoTask>? TasksList { get; set; }
}
