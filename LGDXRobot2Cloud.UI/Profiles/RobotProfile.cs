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
      /*// Nodes Collection
      CreateMap<Blazor.NodesCollectionBlazor, Models.NodesCollectionCreateDto>();
      CreateMap<Blazor.NodesCollectionBlazor, Models.NodesCollectionUpdateDto>();
      CreateMap<Blazor.NodesCollectionDetailBlazor, Models.NodesCollectionDetailCreateDto>();
      CreateMap<Blazor.NodesCollectionDetailBlazor, Models.NodesCollectionDetailUpdateDto>();
      // Robot
      CreateMap<Blazor.RobotBlazor, Models.RobotCreateDto>();
      CreateMap<Blazor.RobotBlazor, Models.RobotUpdateDto>();*/
    }
  }
}