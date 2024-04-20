using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;
using Blazor = LGDXRobot2Cloud.Shared.Models.Blazor;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      // API Key
      CreateMap<Blazor.ApiKeyBlazor, Models.ApiKeyCreateDto>();
      CreateMap<Blazor.ApiKeyBlazor, Models.ApiKeyUpdateDto>();
      CreateMap<Blazor.ApiKeySecretBlazor, Models.ApiKeySecretDto>();
    }
  }
}