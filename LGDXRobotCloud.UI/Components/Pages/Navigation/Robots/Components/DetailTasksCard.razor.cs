using LGDXRobotCloud.UI.Client.Models;
using Microsoft.AspNetCore.Components;

namespace LGDXRobotCloud.UI.Components.Pages.Navigation.Robots.Components;

public partial class DetailTasksCard
{
  [Parameter]
  public IEnumerable<AutoTaskListDto>? AutoTasks { get; set; }
}
