using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Identify;

namespace LGDXRobot2Cloud.API.Profiles;

public class IdentifyProfile : Profile
{
  public IdentifyProfile()
  {
    CreateMap<LgdxUser, LgdxUserDto>();
    CreateMap<LgdxUser, LgdxUserListDto>();
    CreateMap<LgdxUserUpdateDto, LgdxUser>();
  }
}