using AutoMapper;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      CreateMap<Entities.Progress, Models.ProgressDto>();
      CreateMap<Models.ProgressCreateDto, Entities.Progress>();
      CreateMap<Entities.Trigger, Models.TriggerDto>()
        .ForMember(dto => dto.ApiKeyLocation, m => m.MapFrom(e => e.ApiKeyLocation != null ? e.ApiKeyLocation.Name : ""));
      CreateMap<Models.TriggerCreateDto, Entities.Trigger>();
      CreateMap<Entities.Waypoint, Models.WaypointDto>();
      CreateMap<Models.WaypointCreateDto, Entities.Waypoint>();
    }
  }
}