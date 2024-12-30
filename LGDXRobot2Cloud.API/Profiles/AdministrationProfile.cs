using AutoMapper;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.API.Profiles;

public class AdministrationProfile : Profile
{
  public AdministrationProfile()
  {
    // Api Keys
    CreateMap<ApiKey, ApiKeyDto>();
    CreateMap<ApiKeyCreateDto, ApiKey>();
    CreateMap<ApiKeyUpdateDto, ApiKey>();
    CreateMap<ApiKeySecretUpdateDto, ApiKey>();
    CreateMap<ApiKey, ApiKeySecretDto>();

    // Roles
    CreateMap<LgdxRole, LgdxRoleDto>();
    CreateMap<LgdxRole, LgdxRoleListDto>();
    CreateMap<LgdxRoleCreateDto, LgdxRole>();
    CreateMap<LgdxRoleUpdateDto, LgdxRole>();

    CreateMap<RobotCertificate, RobotCertificateListDto>();
    CreateMap<RobotCertificate, RobotCertificateDto>();

    // Users
    CreateMap<LgdxUser, LgdxUserListDto>();
    CreateMap<LgdxUserUpdateAdminDto, LgdxUser>();
    CreateMap<LgdxUserCreateAdminDto, LgdxUser>();
  }
}