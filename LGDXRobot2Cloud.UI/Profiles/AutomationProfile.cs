using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.ViewModels.Automation;

namespace LGDXRobot2Cloud.UI.Profiles;

public class AutomationProfile : Profile
{
  public AutomationProfile()
  {
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