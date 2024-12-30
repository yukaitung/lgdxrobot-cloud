using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.ViewModels.Automation;

namespace LGDXRobot2Cloud.UI.Profiles;

public class AutomationProfile : Profile
{
  public AutomationProfile()
  {
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