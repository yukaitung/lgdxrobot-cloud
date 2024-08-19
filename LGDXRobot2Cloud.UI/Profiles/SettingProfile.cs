using AutoMapper;
using Models = LGDXRobot2Cloud.Data.Models.DTOs;
using Blazor = LGDXRobot2Cloud.Data.Models.Blazor;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      // API Key
      CreateMap<Blazor.ApiKeyBlazor, Models.Commands.ApiKeyCreateDto>();
      CreateMap<Blazor.ApiKeyBlazor, Models.Commands.ApiKeyUpdateDto>();
      CreateMap<Blazor.ApiKeySecretBlazor, Models.Responses.ApiKeySecretDto>();
    }
  }
}