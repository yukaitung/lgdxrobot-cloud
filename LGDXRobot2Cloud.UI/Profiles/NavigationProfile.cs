using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;
using Blazor = LGDXRobot2Cloud.Shared.Models.Blazor;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Progress
      CreateMap<Blazor.ProgressBlazor, Models.ProgressCreateDto>();
      CreateMap<Blazor.ProgressBlazor, Models.ProgressUpdateDto>();
      // Triggers
      CreateMap<Blazor.TriggerBlazor, Models.TriggerCreateDto>();
      CreateMap<Blazor.TriggerBlazor, Models.TriggerUpdateDto>();
      // Waypoint
      CreateMap<Blazor.WaypointBlazor, Models.WaypointCreateDto>();
      CreateMap<Blazor.WaypointBlazor, Models.WaypointUpdateDto>();
    }
  }
}