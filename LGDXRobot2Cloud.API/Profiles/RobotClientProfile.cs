using AutoMapper;
using Entities = LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Profiles;

public class RobotClientProfile : Profile
{
  public RobotClientProfile()
  {
    CreateMap<Entities.RobotSystemInfo, Entities.RobotSystemInfo>()
      .ForMember(m => m.Id, opt => opt.Ignore());
  }
}