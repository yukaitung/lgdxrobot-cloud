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
      // Flow
      CreateMap<Flow, Models.Responses.FlowListDto>();
      CreateMap<Flow, Models.Responses.FlowDto>();
      CreateMap<FlowDetail, Models.Responses.FlowDetailDto>();
      CreateMap<Models.Commands.FlowCreateDto, Models.Commands.FlowUpdateDto>();
      CreateMap<Models.Commands.FlowDetailCreateDto, Models.Commands.FlowDetailUpdateDto>();
      CreateMap<Models.Commands.FlowUpdateDto, Flow>();
      CreateMap<IEnumerable<Models.Commands.FlowDetailUpdateDto>, ICollection<FlowDetail>>()
        .ConvertUsing<FlowDetailUpdateDtoToFlowDetail>();
      CreateMap<Models.Commands.FlowDetailUpdateDto, FlowDetail>();
      // Tasks
      CreateMap<AutoTask, AutoTaskListDto>();
      CreateMap<AutoTask, Models.Responses.AutoTaskDto>();
      CreateMap<Models.Commands.AutoTaskCreateDto, Models.Commands.AutoTaskUpdateDto>();
      CreateMap<Models.Commands.AutoTaskUpdateDto, AutoTask>();
      CreateMap<AutoTaskDetail, Models.Responses.AutoTaskDetailDto>();
      CreateMap<Models.Commands.AutoTaskDetailCreateDto, Models.Commands.AutoTaskDetailUpdateDto>();
      CreateMap<Models.Commands.AutoTaskDetailUpdateDto, AutoTaskDetail>();
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
    }
  }

  public class FlowDetailUpdateDtoToFlowDetail : ITypeConverter<IEnumerable<Models.Commands.FlowDetailUpdateDto>, ICollection<FlowDetail>>
  {
    public ICollection<FlowDetail> Convert(IEnumerable<Models.Commands.FlowDetailUpdateDto> src, ICollection<FlowDetail> dest, ResolutionContext context)
    {
      ICollection<FlowDetail> result = [];
      foreach(Models.Commands.FlowDetailUpdateDto e in src)
      {
        result.Add(context.Mapper.Map<FlowDetail>(e));
      }
      return result;
    }
  }
}