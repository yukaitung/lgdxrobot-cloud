using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.ViewModels.Navigation;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Flow
      CreateMap<Flow, FlowCreateDto>();
      CreateMap<FlowDetail, FlowDetailCreateDto>();
      CreateMap<Flow, FlowUpdateDto>();
      CreateMap<FlowDetail, FlowDetailUpdateDto>();
      // Tasks
      CreateMap<AutoTask, AutoTaskCreateDto>();
      CreateMap<AutoTaskDetail, AutoTaskDetailCreateDto>();
      CreateMap<AutoTask, AutoTaskUpdateDto>();
      CreateMap<AutoTaskDetail, AutoTaskDetailUpdateDto>();
      // Waypoint
      CreateMap<WaypointDto, WaypointDetailViewModel>()
        .ForMember(d => d.RealmId, opt => opt.MapFrom(s => s.Realm.Id))
        .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Realm.Name));
      CreateMap<WaypointDetailViewModel, WaypointCreateDto>();
      CreateMap<WaypointDetailViewModel, WaypointUpdateDto>();
      // Maps
      CreateMap<RealmDetailViewModel, RealmCreateDto>();
      CreateMap<RealmDetailViewModel, RealmUpdateDto>();
      CreateMap<RealmDto, RealmDetailViewModel>();
      // Robots
      CreateMap<RobotChassisInfoDto, RobotChassisInfoViewModel>();
      CreateMap<RobotChassisInfoViewModel, RobotChassisInfoCreateDto>();
      CreateMap<RobotChassisInfoViewModel, RobotChassisInfoUpdateDto>();
      CreateMap<RobotDetailViewModel, RobotCreateDto>();
      CreateMap<RobotDto, RobotDetailViewModel>()
        .ForMember(d => d.RealmId, opt => opt.MapFrom(s => s.Realm.Id))
        .ForMember(d => d.RealmName, opt => opt.MapFrom(s => s.Realm.Name));
    }
  }
}