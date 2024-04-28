using LGDXRobot2Cloud.Shared.Models.Blazor;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.RobotDetail
{
  public partial class TasksTable
  {

    [Parameter]
    public ICollection<AutoTaskBlazor>? TasksList { get; set; }
  }
}