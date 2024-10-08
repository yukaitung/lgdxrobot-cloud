using AutoMapper;
using Entities = LGDXRobot2Cloud.Data.Entities;
using Models = LGDXRobot2Cloud.Data.Models.DTOs;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Robot Management
      CreateMap<Entities.Robot, Models.Responses.RobotListDto>();
      CreateMap<Entities.Robot, Models.Responses.RobotDto>();
      CreateMap<Models.Commands.RobotCreateInfoDto, Entities.Robot>();
      CreateMap<Models.Commands.RobotCreateChassisInfoDto, Entities.RobotChassisInfo>();
      CreateMap<Models.Commands.RobotChassisInfoUpdateDto, Entities.RobotChassisInfo>();
      CreateMap<Models.Commands.RobotUpdateDto, Entities.Robot>();
      CreateMap<Entities.RobotSystemInfo, Models.Responses.RobotSystemInfoDto>();
      CreateMap<Entities.RobotChassisInfo, Models.Responses.RobotChassisInfoDto>();
      // Nodes
      CreateMap<Entities.Node, Models.Responses.NodeDto>();
      CreateMap<Models.Commands.NodeCreateDto, Entities.Node>();
      CreateMap<Models.Commands.NodeUpdateDto, Entities.Node>();
      // Nodes Collection
      CreateMap<Entities.NodesCollection, Models.Responses.NodesCollectionListDto>();
      CreateMap<Entities.NodesCollection, Models.Responses.NodesCollectionDto>();
      CreateMap<Entities.NodesCollectionDetail, Models.Responses.NodesCollectionDetailDto>();
      CreateMap<Models.Commands.NodesCollectionCreateDto, Models.Commands.NodesCollectionUpdateDto>();
      CreateMap<Models.Commands.NodesCollectionUpdateDto, Entities.NodesCollection>();
      CreateMap<Models.Responses.NodesCollectionDetailDto, Entities.NodesCollection>();
      CreateMap<Models.Commands.NodesCollectionDetailCreateDto, Models.Commands.NodesCollectionDetailUpdateDto>();
      CreateMap<Models.Commands.NodesCollectionDetailUpdateDto, Entities.NodesCollectionDetail>();
    }
  }
}