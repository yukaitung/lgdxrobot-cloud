using AutoMapper;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      

      // User
      CreateMap<LgdxUser, LgdxUserCreateAdminDto>();
      CreateMap<LgdxUser, LgdxUserUpdateDto>();
      CreateMap<LgdxUser, LgdxUserUpdateAdminDto>();

      // Role
      CreateMap<LgdxRole, LgdxRoleCreateDto>();
      CreateMap<LgdxRole, LgdxRoleUpdateDto>();
    }
  }
}