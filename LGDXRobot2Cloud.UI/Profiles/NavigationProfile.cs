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
      CreateMap<Flow, FlowCreateDto>();
      CreateMap<FlowDetail, FlowDetailCreateDto>();
      CreateMap<Flow, FlowUpdateDto>();
      CreateMap<FlowDetail, FlowDetailUpdateDto>();
      // Progress
      CreateMap<Progress, ProgressCreateDto>();
      CreateMap<Progress, ProgressUpdateDto>();
      // Tasks
      CreateMap<AutoTask, AutoTaskCreateDto>();
      CreateMap<AutoTaskDetail, AutoTaskDetailCreateDto>();
      CreateMap<AutoTask, AutoTaskUpdateDto>();
      CreateMap<AutoTaskDetail, AutoTaskDetailUpdateDto>();
      // Triggers
      CreateMap<Trigger, TriggerCreateDto>();
      CreateMap<Trigger, TriggerUpdateDto>();
      // Waypoint
      CreateMap<Waypoint, WaypointCreateDto>();
      CreateMap<Waypoint, WaypointUpdateDto>();
    }
  }
}