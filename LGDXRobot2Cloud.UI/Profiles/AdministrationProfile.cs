using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.ViewModels.Administration;

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

    // Robot Certificates
    CreateMap<RobotCertificateRenewViewModel, RobotCertificateRenewRequestDto>();

    // Users
    CreateMap<LgdxUserDto, UserDetailViewModel>();
    CreateMap<UserDetailViewModel, LgdxUserUpdateAdminDto>();
    CreateMap<UserDetailViewModel, LgdxUserCreateAdminDto>();
  }
}