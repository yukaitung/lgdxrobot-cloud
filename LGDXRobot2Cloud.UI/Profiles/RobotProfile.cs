using AutoMapper;
using Models = LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using Blazor = LGDXRobot2Cloud.Data.Models.Blazor;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Nodes
      CreateMap<Blazor.NodeBlazor, Models.NodeCreateDto>();
      CreateMap<Blazor.NodeBlazor, Models.NodeUpdateDto>();
      // Nodes Collection
      CreateMap<Blazor.NodesCollectionBlazor, Models.NodesCollectionCreateDto>();
      CreateMap<Blazor.NodesCollectionBlazor, Models.NodesCollectionUpdateDto>();
      CreateMap<Blazor.NodesCollectionDetailBlazor, Models.NodesCollectionDetailCreateDto>();
      CreateMap<Blazor.NodesCollectionDetailBlazor, Models.NodesCollectionDetailUpdateDto>();
      // Robot
      CreateMap<Blazor.RobotBlazor, Models.RobotCreateDto>();
      CreateMap<Blazor.RobotBlazor, Models.RobotUpdateDto>();
    }
  }
}