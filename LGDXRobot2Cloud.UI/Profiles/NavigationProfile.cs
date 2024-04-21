using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;
using Blazor = LGDXRobot2Cloud.Shared.Models.Blazor;
using Entities = LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Progress
      CreateMap<Blazor.ProgressBlazor, Models.ProgressCreateDto>();
      CreateMap<Blazor.ProgressBlazor, Models.ProgressUpdateDto>();
      // Waypoint
      CreateMap<Entities.Waypoint, Models.WaypointCreateDto>();
      CreateMap<Entities.Waypoint, Models.WaypointUpdateDto>();
    }
  }
}