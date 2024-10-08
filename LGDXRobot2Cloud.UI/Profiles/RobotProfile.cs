using AutoMapper;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Nodes
      CreateMap<Node, NodeCreateDto>();
      CreateMap<Node, NodeUpdateDto>();
      // Nodes Collection
      CreateMap<NodesCollection, NodesCollectionCreateDto>();
      CreateMap<NodesCollection, NodesCollectionUpdateDto>();
      CreateMap<NodesCollectionDetail, NodesCollectionDetailCreateDto>();
      CreateMap<NodesCollectionDetail, NodesCollectionDetailUpdateDto>();
      // Robot
      CreateMap<Robot, RobotCreateInfoDto>();
      CreateMap<RobotChassisInfo, RobotCreateChassisInfo>();
      CreateMap<Robot, RobotUpdateDto>();
    }
  }
}