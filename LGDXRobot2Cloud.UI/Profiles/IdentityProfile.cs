using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.ViewModels.Identity;

namespace LGDXRobot2Cloud.UI.Profiles;

public class IdentityProfile : Profile
{
  public IdentityProfile()
  {
    CreateMap<LoginViewModel, LoginRequestDto>();
    CreateMap<ForgotPasswordViewModel, ForgotPasswordRequestDto>();
    CreateMap<ResetPasswordViewModel, ResetPasswordRequestDto>();
    CreateMap<LgdxUserDto, UserDetailViewModel>();
    CreateMap<UserDetailViewModel, LgdxUserUpdateDto>();
    CreateMap<UserDetailPasswordViewModel, UpdatePasswordRequestDto>();
  }
}