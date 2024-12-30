using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.API.Profiles;

public class AutomationProfile : Profile
{
  public AutomationProfile()
  {
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