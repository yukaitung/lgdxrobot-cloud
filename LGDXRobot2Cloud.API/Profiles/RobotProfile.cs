using AutoMapper;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Robot Management
      CreateMap<Entities.Robot, Models.RobotListDto>();
      CreateMap<Entities.Robot, Models.RobotDto>();
      CreateMap<Models.RobotCreateDto, Entities.Robot>();
      // Nodes
      CreateMap<Entities.Node, Models.NodeDto>();
      CreateMap<Models.NodeCreateDto, Entities.Node>();
    }
  }
}