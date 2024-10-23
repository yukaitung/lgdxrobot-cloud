using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Identity;

namespace LGDXRobot2Cloud.API.Profiles;

public class IdentityProfile : Profile
{
  public IdentityProfile()
  {
    CreateMap<LgdxUser, LgdxUserDto>();
    CreateMap<LgdxUser, LgdxUserListDto>();
    CreateMap<LgdxUserUpdateDto, LgdxUser>();
    CreateMap<LgdxRole, LgdxRoleDto>();
    CreateMap<LgdxRole, LgdxRoleListDto>();
    CreateMap<LgdxRoleCreateDto, LgdxRole>();
    CreateMap<LgdxRoleUpdateDto, LgdxRole>();
  }
}