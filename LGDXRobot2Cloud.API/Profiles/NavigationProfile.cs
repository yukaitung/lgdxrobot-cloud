using AutoMapper;
using Entities = LGDXRobot2Cloud.Shared.Entities;
using Models = LGDXRobot2Cloud.Shared.Models;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Flow
      CreateMap<Entities.Flow, Models.FlowListDto>();
      CreateMap<Entities.Flow, Models.FlowDto>();
      CreateMap<Entities.FlowDetail, Models.FlowDetailDto>()
        .ForMember(dto => dto.ProceedCondition,
          m => m.MapFrom(e => e.ProceedCondition.Name));
      CreateMap<Models.FlowCreateDto, Entities.Flow>();
      CreateMap<IEnumerable<Models.FlowDetailCreateDto>, ICollection<Entities.FlowDetail>>()
        .ConvertUsing<FlowDetailCreateDtoToFlowDetail>();
      CreateMap<Models.FlowDetailCreateDto, Entities.FlowDetail>()
        .ForMember(e => e.ProceedCondition, m => m.Ignore());
      CreateMap<Models.FlowUpdateDto, Entities.Flow>();
      CreateMap<IEnumerable<Models.FlowDetailUpdateDto>, ICollection<Entities.FlowDetail>>()
        .ConvertUsing<FlowDetailUpdateDtoToFlowDetail>();
      CreateMap<Models.FlowDetailUpdateDto, Entities.FlowDetail>()
        .ForMember(e => e.ProceedCondition, m => m.Ignore());
      // Progress
      CreateMap<Entities.Progress, Models.ProgressDto>();
      CreateMap<Models.ProgressCreateDto, Entities.Progress>();
      // Tasks
      CreateMap<Entities.AutoTask, Models.AutoTaskListDto>();
      CreateMap<Entities.AutoTask, Models.AutoTaskDto>();
      CreateMap<Models.AutoTaskCreateDto, Entities.AutoTask>();
      CreateMap<Models.AutoTaskUpdateDto, Entities.AutoTask>();
      CreateMap<Entities.AutoTaskWaypointDetail, Models.AutoTaskWaypointDetailDto>();
      CreateMap<Models.AutoTaskWaypointDetailCreateDto, Entities.AutoTaskWaypointDetail>();
      CreateMap<Models.AutoTaskWaypointDetailUpdateDto, Entities.AutoTaskWaypointDetail>();
      // Trigger
      CreateMap<Entities.Trigger, Models.TriggerDto>()
        .ForMember(dto => dto.ApiKeyLocation,
          m => m.MapFrom(e => e.ApiKeyLocation != null ? e.ApiKeyLocation.Name : ""));
      CreateMap<Models.TriggerCreateDto, Entities.Trigger>();
      // Waypoint
      CreateMap<Entities.Waypoint, Models.WaypointDto>();
      CreateMap<Models.WaypointCreateDto, Entities.Waypoint>();
    }
  }

  public class FlowDetailCreateDtoToFlowDetail : ITypeConverter<IEnumerable<Models.FlowDetailCreateDto>, ICollection<Entities.FlowDetail>>
  {
    public ICollection<Entities.FlowDetail> Convert(IEnumerable<Models.FlowDetailCreateDto> src, ICollection<Entities.FlowDetail> dest, ResolutionContext context)
    {
      ICollection<Entities.FlowDetail> result = new List<Entities.FlowDetail>();
      foreach(Models.FlowDetailCreateDto e in src)
      {
        result.Add(context.Mapper.Map<Entities.FlowDetail>(e));
      }
      return result;
    }
  }

  public class FlowDetailUpdateDtoToFlowDetail : ITypeConverter<IEnumerable<Models.FlowDetailUpdateDto>, ICollection<Entities.FlowDetail>>
  {
    public ICollection<Entities.FlowDetail> Convert(IEnumerable<Models.FlowDetailUpdateDto> src, ICollection<Entities.FlowDetail> dest, ResolutionContext context)
    {
      ICollection<Entities.FlowDetail> result = new List<Entities.FlowDetail>();
      foreach(Models.FlowDetailUpdateDto e in src)
      {
        result.Add(context.Mapper.Map<Entities.FlowDetail>(e));
      }
      return result;
    }
  }
}