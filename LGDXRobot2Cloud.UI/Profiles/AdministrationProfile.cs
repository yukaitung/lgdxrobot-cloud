using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.ViewModels.Administration;
using LGDXRobot2Cloud.UI.ViewModels.Administration.Roles;
using LGDXRobot2Cloud.UI.ViewModels.Administration.Users;

namespace LGDXRobot2Cloud.UI.Profiles;

public class AdministrationProfile : Profile
{
  public AdministrationProfile()
  {
    // API Key
    CreateMap<ApiKeyDto, ApiKeyDetailViewModel>();
    CreateMap<ApiKeyDetailViewModel, ApiKeyCreateDto>();
    CreateMap<ApiKeyDetailViewModel, ApiKeyUpdateDto>();

    // Roles
    CreateMap<LgdxRoleDto, RolesDetailViewModel>();
    CreateMap<RolesDetailViewModel, LgdxRoleUpdateDto>();
    CreateMap<RolesDetailViewModel, LgdxRoleCreateDto>();

    // Users
    CreateMap<LgdxUserDto, UserDetailViewModel>();
    CreateMap<UserDetailViewModel, LgdxUserUpdateAdminDto>();
    CreateMap<UserDetailViewModel, LgdxUserCreateAdminDto>();
  }
}