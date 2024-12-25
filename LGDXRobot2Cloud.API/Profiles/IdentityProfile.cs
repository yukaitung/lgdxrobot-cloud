using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Identity;

namespace LGDXRobot2Cloud.API.Profiles;

public class IdentityProfile : Profile
{
  public IdentityProfile()
  {
    CreateMap<LgdxRole, LgdxRoleDto>();
    CreateMap<LgdxRole, LgdxRoleListDto>();
    CreateMap<LgdxRoleCreateDto, LgdxRole>();
    CreateMap<LgdxRoleUpdateDto, LgdxRole>();
    CreateMap<LgdxUser, LgdxUserDto>();
    CreateMap<LgdxUser, LgdxUserListDto>();
    CreateMap<LgdxUserUpdateAdminDto, LgdxUser>();
    CreateMap<LgdxUserUpdateDto, LgdxUser>();
  }
}