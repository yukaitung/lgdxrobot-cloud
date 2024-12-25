using AutoMapper;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.Identity;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      // API Key
      CreateMap<ApiKey, ApiKeyCreateDto>();
      CreateMap<ApiKey, ApiKeyUpdateDto>();
      CreateMap<ApiKeySecret, ApiKeySecretDto>();

      // User
      CreateMap<LgdxUser, LgdxUserCreateDto>();
      CreateMap<LgdxUser, LgdxUserUpdateDto>();
      CreateMap<LgdxUser, LgdxUserUpdateAdminDto>();

      // Role
      CreateMap<LgdxRole, LgdxRoleCreateDto>();
      CreateMap<LgdxRole, LgdxRoleUpdateDto>();
    }
  }
}