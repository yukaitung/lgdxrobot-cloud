using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Models = LGDXRobot2Cloud.Data.Models.DTOs;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Waypoint
      CreateMap<Waypoint, WaypointDto>();
      CreateMap<Waypoint, WaypointListDto>();
      CreateMap<Waypoint, WaypointSearchDto>();
      CreateMap<WaypointCreateDto, Waypoint>();
      CreateMap<WaypointUpdateDto, Waypoint>();
      // Realm
      CreateMap<Realm, RealmDto>()
        .ForMember(d => d.Image, opt => opt.MapFrom(s => Convert.ToBase64String(s.Image)));
      CreateMap<Realm, RealmListDto>();
      CreateMap<Realm, RealmSearchDto>();
      CreateMap<RealmCreateDto, Realm>()
        .ForMember(d => d.Image, opt => opt.MapFrom(s => Convert.FromBase64String(s.Image)));
      CreateMap<RealmUpdateDto, Realm>()
        .ForMember(d => d.Image, opt => opt.MapFrom(s => Convert.FromBase64String(s.Image)));
      // Robots
      CreateMap<Robot, RobotDto>();
      CreateMap<Robot, RobotListDto>();
      CreateMap<RobotChassisInfoCreateDto, RobotChassisInfo>();
      CreateMap<RobotChassisInfoUpdateDto, RobotChassisInfo>();
      CreateMap<RobotCreateDto, Robot>();
      CreateMap<RobotUpdateDto, Robot>();
      CreateMap<Robot, RobotSearchDto>();
      CreateMap<RobotChassisInfo, RobotChassisInfoDto>();
      CreateMap<RobotSystemInfo, RobotSystemInfoDto>();
    }
  }
}