using AutoMapper;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Robot
      CreateMap<Robot, RobotCreateInfoDto>();
      CreateMap<RobotChassisInfo, RobotCreateChassisInfoDto>();
      CreateMap<RobotChassisInfo, RobotChassisInfoUpdateDto>();
      CreateMap<Robot, RobotUpdateDto>();
    }
  }
}