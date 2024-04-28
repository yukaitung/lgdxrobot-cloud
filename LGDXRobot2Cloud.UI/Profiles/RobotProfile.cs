using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;
using Blazor = LGDXRobot2Cloud.Shared.Models.Blazor;

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
    }
  }
}