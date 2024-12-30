using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.ViewModels.Automation;

namespace LGDXRobot2Cloud.UI.Profiles;

public class AutomationProfile : Profile
{
  public AutomationProfile()
  {
    // AutoTasks
    CreateMap<AutoTaskDetailViewModel, AutoTaskCreateDto>();
    CreateMap<TaskDetailBody, AutoTaskDetailCreateDto>();
    CreateMap<AutoTaskDetailViewModel, AutoTaskUpdateDto>();
    CreateMap<TaskDetailBody, AutoTaskDetailUpdateDto>();
    CreateMap<AutoTaskDto, AutoTaskDetailViewModel>()
      .ForMember(d => d.FlowId, opt => opt.MapFrom(s => s.Flow.Id))
      .ForMember(d => d.FlowName, opt => opt.MapFrom(s => s.Flow.Name))
      .ForMember(d => d.RealmId, opt => opt.MapFrom(s => s.Realm.Id))
      .ForMember(d => d.RealmName, opt => opt.MapFrom(s => s.Realm.Name))
      .ForMember(d => d.AssignedRobotId, opt => opt.MapFrom(s => s.AssignedRobot!.Id))
      .ForMember(d => d.AssignedRobotName, opt => opt.MapFrom(s => s.AssignedRobot!.Name))
      .ForMember(d => d.CurrentProgressId, opt => opt.MapFrom(s => s.CurrentProgress.Id))
      .ForMember(d => d.CurrentProgressName, opt => opt.MapFrom(s => s.CurrentProgress.Name));
    CreateMap<AutoTaskDetailDto, TaskDetailBody>()
      .ForMember(d => d.WaypointId, opt => opt.MapFrom(s => s.Waypoint!.Id))
      .ForMember(d => d.WaypointName, opt => opt.MapFrom(s => s.Waypoint!.Name));

    // Flow
    CreateMap<FlowDetailViewModel, FlowCreateDto>();
    CreateMap<FlowDetailBody, FlowDetailCreateDto>();
    CreateMap<FlowDetailViewModel, FlowUpdateDto>();
    CreateMap<FlowDetailBody, FlowDetailUpdateDto>();
    CreateMap<FlowDto, FlowDetailViewModel>();
    CreateMap<FlowDetailDto, FlowDetailBody>()
      .ForMember(d => d.ProgressId, opt => opt.MapFrom(s => s.Progress.Id))
      .ForMember(d => d.ProgressName, opt => opt.MapFrom(s => s.Progress.Name))
      .ForMember(d => d.TriggerId, opt => opt.MapFrom(s => s.Trigger!.Id))
      .ForMember(d => d.TriggerName, opt => opt.MapFrom(s => s.Trigger!.Name));

    // Progress
    CreateMap<ProgressDto, ProgressDetailViewModel>();
    CreateMap<ProgressDetailViewModel, ProgressCreateDto>();
    CreateMap<ProgressDetailViewModel, ProgressUpdateDto>();

    // Trigger
    CreateMap<TriggerDto, TriggerDetailViewModel>();
    CreateMap<TriggerDetailViewModel, TriggerCreateDto>();
    CreateMap<TriggerDetailViewModel, TriggerUpdateDto>();
  }
}