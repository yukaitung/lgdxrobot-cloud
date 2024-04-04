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
      // Nodes Collection
      CreateMap<Entities.NodesCollection, Models.NodesCollectionListDto>();
      CreateMap<Entities.NodesCollection, Models.NodesCollectionDto>();
      CreateMap<Entities.NodesCollectionDetail, Models.NodesCollectionDetailDto>();
      CreateMap<Models.NodesCollectionCreateDto, Entities.NodesCollection>();
      CreateMap<Models.NodesCollectionUpdateDto, Entities.NodesCollection>();
      CreateMap<Models.NodesCollectionDetailDto, Entities.NodesCollection>();
      CreateMap<Models.NodesCollectionDetailCreateDto, Entities.NodesCollectionDetail>();
      CreateMap<Models.NodesCollectionDetailUpdateDto, Entities.NodesCollectionDetail>();
    }
  }
}