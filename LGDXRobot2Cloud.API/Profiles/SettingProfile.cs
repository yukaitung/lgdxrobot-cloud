using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using Entities = LGDXRobot2Cloud.Data.Entities;
using Models = LGDXRobot2Cloud.Data.Models.DTOs;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {


      CreateMap<Entities.RobotCertificate, Models.Responses.RobotCertificateListDto>();
      CreateMap<Entities.RobotCertificate, RobotCertificateDto>();
    }
  }
}