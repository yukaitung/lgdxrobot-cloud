using AutoMapper;
using Entities = LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Profiles;

public class RobotClientProfile : Profile
{
  public RobotClientProfile()
  {
    CreateMap<Entities.RobotSystemInfo, Entities.RobotSystemInfo>()
      .ForMember(m => m.Id, opt => opt.Ignore())
      .ForMember(m => m.RobotId, opt => opt.Ignore())
      .ForMember(m => m.Robot, opt => opt.Ignore())
      .ForMember(m => m.CreatedAt, opt => opt.Ignore())
      .ForMember(m => m.UpdatedAt, opt => opt.Ignore());
  }
}