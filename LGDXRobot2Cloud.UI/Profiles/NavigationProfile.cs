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
      //CreateMap<Blazor.AutoTaskBlazor, Models.AutoTaskCreateDto>();
      //CreateMap<Blazor.AutoTaskDetailBlazor, Models.AutoTaskDetailCreateDto>();
      //CreateMap<Blazor.AutoTaskBlazor, Models.AutoTaskUpdateDto>();
      //CreateMap<Blazor.AutoTaskDetailBlazor, Models.AutoTaskDetailUpdateDto>();
      // Triggers
      CreateMap<Trigger, TriggerCreateDto>();
      CreateMap<Trigger, TriggerUpdateDto>();
      // Waypoint
      CreateMap<Waypoint, WaypointCreateDto>();
      CreateMap<Waypoint, WaypointUpdateDto>();
    }
  }
}