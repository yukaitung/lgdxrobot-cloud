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
    CreateMap<ApiKey, ApiKeySecretDto>();
    CreateMap<ApiKey, ApiKeySearchDto>();
    CreateMap<ApiKeyCreateDto, ApiKey>();
    CreateMap<ApiKeySecretUpdateDto, ApiKey>();
    CreateMap<ApiKeyUpdateDto, ApiKey>();

    // Roles
    CreateMap<LgdxRole, LgdxRoleDto>();
    CreateMap<LgdxRole, LgdxRoleListDto>();
    CreateMap<LgdxRole, LgdxRoleSearchDto>();
    CreateMap<LgdxRoleCreateDto, LgdxRole>();
    CreateMap<LgdxRoleUpdateDto, LgdxRole>();

    // Robot Certificate
    CreateMap<RobotCertificate, RobotCertificateListDto>();
    CreateMap<RobotCertificate, RobotCertificateDto>();

    // Users
    CreateMap<LgdxUser, LgdxUserListDto>();
    CreateMap<LgdxUserUpdateAdminDto, LgdxUser>();
    CreateMap<LgdxUserCreateAdminDto, LgdxUser>();
  }
}