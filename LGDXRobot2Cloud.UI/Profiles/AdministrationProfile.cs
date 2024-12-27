using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.ViewModels.Administration.Roles;

namespace LGDXRobot2Cloud.UI.Profiles;

public class AdministrationProfile : Profile
{
  public AdministrationProfile()
  {
    CreateMap<LgdxRoleDto, RolesDetailViewModel>();
    CreateMap<RolesDetailViewModel, LgdxRoleUpdateDto>();
    CreateMap<RolesDetailViewModel, LgdxRoleCreateDto>();
  }
}