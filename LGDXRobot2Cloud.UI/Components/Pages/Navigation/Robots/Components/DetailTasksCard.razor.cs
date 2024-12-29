using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Robots.Components;

public partial class DetailTasksCard
{
  [Parameter]
  public IEnumerable<AutoTaskListDto>? AutoTasks { get; set; }
}
