using AutoMapper;
using Entities = LGDXRobot2Cloud.Data.Entities;
using Models = LGDXRobot2Cloud.Data.Models.DTOs;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class RobotProfile : Profile
  {
    public RobotProfile()
    {
      // Robot Management
      CreateMap<Entities.Robot, Models.Responses.RobotListDto>();
      CreateMap<Entities.Robot, Models.Responses.RobotDto>();
      CreateMap<Models.Commands.RobotCreateInfoDto, Entities.Robot>();
      CreateMap<Models.Commands.RobotCreateChassisInfoDto, Entities.RobotChassisInfo>();
      CreateMap<Models.Commands.RobotChassisInfoUpdateDto, Entities.RobotChassisInfo>();
      CreateMap<Models.Commands.RobotUpdateDto, Entities.Robot>();
      CreateMap<Entities.RobotSystemInfo, Models.Responses.RobotSystemInfoDto>();
      CreateMap<Entities.RobotChassisInfo, Models.Responses.RobotChassisInfoDto>();
    }
  }
}