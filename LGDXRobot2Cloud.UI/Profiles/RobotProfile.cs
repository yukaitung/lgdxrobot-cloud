using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Nodes
      CreateMap<Models.NodeDto, Models.NodeCreateDto>();
    }
  }
}