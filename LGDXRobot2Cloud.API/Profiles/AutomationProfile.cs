using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.API.Profiles;

public class AutomationProfile : Profile
{
  public AutomationProfile()
  {
    // AutoTasks
    CreateMap<AutoTask, AutoTaskListDto>();
    CreateMap<AutoTask, AutoTaskDto>();
    CreateMap<AutoTaskCreateDto, AutoTask>();
    CreateMap<AutoTaskUpdateDto, AutoTask>();
    CreateMap<AutoTaskDetail, AutoTaskDetailDto>();
    CreateMap<AutoTaskDetailCreateDto, AutoTaskDetail>();
    CreateMap<AutoTaskDetailUpdateDto, AutoTaskDetail>();

    // Flow
    CreateMap<Flow, FlowDto>();
    CreateMap<Flow, FlowListDto>();
    CreateMap<Flow, FlowSearchDto>();
    CreateMap<FlowCreateDto, Flow>();
    CreateMap<FlowDetail, FlowDetailDto>();
    CreateMap<FlowDetailCreateDto, FlowDetail>();
    CreateMap<FlowDetailUpdateDto, FlowDetail>();
    CreateMap<FlowUpdateDto, Flow>();

    // Progress
    CreateMap<Progress, ProgressDto>();
    CreateMap<Progress, ProgressSearchDto>();
    CreateMap<ProgressCreateDto, Progress>();
    CreateMap<ProgressUpdateDto, Progress>();
    
    // Trigger
    CreateMap<Trigger, TriggerDto>();
    CreateMap<Trigger, TriggerListDto>();
    CreateMap<Trigger, TriggerSearchDto>();
    CreateMap<TriggerCreateDto, Trigger>();
    CreateMap<TriggerUpdateDto, Trigger>();
  }
}