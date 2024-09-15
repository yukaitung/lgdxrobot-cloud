using AutoMapper;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;

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
    }
  }
}