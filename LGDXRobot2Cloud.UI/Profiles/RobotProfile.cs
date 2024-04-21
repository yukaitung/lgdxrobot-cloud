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
    }
  }
}