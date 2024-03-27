using AutoMapper;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      CreateMap<Entities.Waypoint, Models.WaypointDto>();
      CreateMap<Models.WaypointCreateDto, Entities.Waypoint>();
    }
  }
}