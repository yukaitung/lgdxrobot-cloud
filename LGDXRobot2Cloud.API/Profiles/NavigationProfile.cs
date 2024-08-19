using AutoMapper;
using Entities = LGDXRobot2Cloud.Data.Entities;
using Models = LGDXRobot2Cloud.Data.Models.DTOs;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class NavigationProfile : Profile
  {
    public NavigationProfile()
    {
      // Flow
      CreateMap<Entities.Flow, Models.Responses.FlowListDto>();
      CreateMap<Entities.Flow, Models.Responses.FlowDto>();
      CreateMap<Entities.FlowDetail, Models.Responses.FlowDetailDto>();
      CreateMap<Models.Commands.FlowCreateDto, Entities.Flow>();
      CreateMap<IEnumerable<Models.Commands.FlowDetailCreateDto>, ICollection<Entities.FlowDetail>>()
        .ConvertUsing<FlowDetailCreateDtoToFlowDetail>();
      CreateMap<Models.Commands.FlowDetailCreateDto, Entities.FlowDetail>();
      CreateMap<Models.Commands.FlowUpdateDto, Entities.Flow>();
      CreateMap<IEnumerable<Models.Commands.FlowDetailUpdateDto>, ICollection<Entities.FlowDetail>>()
        .ConvertUsing<FlowDetailUpdateDtoToFlowDetail>();
      CreateMap<Models.Commands.FlowDetailUpdateDto, Entities.FlowDetail>();
      // Progress
      CreateMap<Entities.Progress, Models.Responses.ProgressDto>();
      CreateMap<Models.Commands.ProgressCreateDto, Entities.Progress>();
      CreateMap<Models.Commands.ProgressUpdateDto, Entities.Progress>();
      // Tasks
      CreateMap<Entities.AutoTask, Models.Responses.AutoTaskListDto>();
      CreateMap<Entities.AutoTask, Models.Responses.AutoTaskDto>();
      CreateMap<Models.Commands.AutoTaskCreateDto, Entities.AutoTask>();
      CreateMap<Models.Commands.AutoTaskUpdateDto, Entities.AutoTask>();
      CreateMap<Entities.AutoTaskDetail, Models.Responses.AutoTaskDetailDto>();
      CreateMap<Models.Commands.AutoTaskDetailCreateDto, Entities.AutoTaskDetail>();
      CreateMap<Models.Commands.AutoTaskDetailUpdateDto, Entities.AutoTaskDetail>();
      // Trigger
      CreateMap<Entities.Trigger, Models.Responses.TriggerListDto>();
      CreateMap<Entities.Trigger, Models.Responses.TriggerDto>();
      CreateMap<Models.Commands.TriggerCreateDto, Entities.Trigger>();
      CreateMap<Models.Commands.TriggerUpdateDto, Entities.Trigger>();
      // Waypoint
      CreateMap<Entities.Waypoint, Models.Responses.WaypointDto>();
      CreateMap<Models.Commands.WaypointCreateDto, Entities.Waypoint>();
      CreateMap<Models.Commands.WaypointUpdateDto, Entities.Waypoint>();
    }
  }

  public class FlowDetailCreateDtoToFlowDetail : ITypeConverter<IEnumerable<Models.Commands.FlowDetailCreateDto>, ICollection<Entities.FlowDetail>>
  {
    public ICollection<Entities.FlowDetail> Convert(IEnumerable<Models.Commands.FlowDetailCreateDto> src, ICollection<Entities.FlowDetail> dest, ResolutionContext context)
    {
      ICollection<Entities.FlowDetail> result = new List<Entities.FlowDetail>();
      foreach(Models.Commands.FlowDetailCreateDto e in src)
      {
        result.Add(context.Mapper.Map<Entities.FlowDetail>(e));
      }
      return result;
    }
  }

  public class FlowDetailUpdateDtoToFlowDetail : ITypeConverter<IEnumerable<Models.Commands.FlowDetailUpdateDto>, ICollection<Entities.FlowDetail>>
  {
    public ICollection<Entities.FlowDetail> Convert(IEnumerable<Models.Commands.FlowDetailUpdateDto> src, ICollection<Entities.FlowDetail> dest, ResolutionContext context)
    {
      ICollection<Entities.FlowDetail> result = new List<Entities.FlowDetail>();
      foreach(Models.Commands.FlowDetailUpdateDto e in src)
      {
        result.Add(context.Mapper.Map<Entities.FlowDetail>(e));
      }
      return result;
    }
  }
}