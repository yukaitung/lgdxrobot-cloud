using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;
using Entities = LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Nodes
      CreateMap<Entities.Waypoint, Models.WaypointCreateDto>();
    }
  }
}