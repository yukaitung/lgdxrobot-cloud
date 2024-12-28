using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.API.Profiles;

public class IdentityProfile : Profile
{
  public IdentityProfile()
  {
    CreateMap<LgdxUser, LgdxUserDto>();
    CreateMap<LgdxUserUpdateDto, LgdxUser>();
  }
}