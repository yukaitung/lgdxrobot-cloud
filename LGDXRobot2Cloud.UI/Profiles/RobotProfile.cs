using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;
using Entities = LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Nodes
      CreateMap<Entities.Node, Models.NodeCreateDto>();
    }
  }
}