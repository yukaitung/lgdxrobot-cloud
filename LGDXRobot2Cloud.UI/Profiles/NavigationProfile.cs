using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Models;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Flow
      //CreateMap<Blazor.FlowBlazor, Models.FlowCreateDto>();
      //CreateMap<Blazor.FlowDetailBlazor, Models.FlowDetailCreateDto>();
      //CreateMap<Blazor.FlowBlazor, Models.FlowUpdateDto>();
      //CreateMap<Blazor.FlowDetailBlazor, Models.FlowDetailUpdateDto>();
      // Progress
      CreateMap<Progress, ProgressCreateDto>();
      CreateMap<Progress, ProgressUpdateDto>();
      // Tasks
      /*CreateMap<Blazor.AutoTaskBlazor, Models.AutoTaskCreateDto>();
      CreateMap<Blazor.AutoTaskDetailBlazor, Models.AutoTaskDetailCreateDto>();
      CreateMap<Blazor.AutoTaskBlazor, Models.AutoTaskUpdateDto>();
      CreateMap<Blazor.AutoTaskDetailBlazor, Models.AutoTaskDetailUpdateDto>();
      // Triggers
      CreateMap<Blazor.TriggerBlazor, Models.TriggerCreateDto>();
      CreateMap<Blazor.TriggerBlazor, Models.TriggerUpdateDto>();
      // Waypoint
      CreateMap<Blazor.WaypointBlazor, Models.WaypointCreateDto>();
      CreateMap<Blazor.WaypointBlazor, Models.WaypointUpdateDto>();*/
    }
  }
}