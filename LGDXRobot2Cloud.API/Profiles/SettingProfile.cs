using AutoMapper;
using Entities = LGDXRobot2Cloud.Shared.Entities;
using Models = LGDXRobot2Cloud.Shared.Models;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      CreateMap<Entities.ApiKey, Models.ApiKeyDto>();
      CreateMap<Models.ApiKeyCreateDto, Entities.ApiKey>();
      CreateMap<Models.ApiKeyUpdateDto, Entities.ApiKey>();
      CreateMap<Models.ApiKeySecretDto, Entities.ApiKey>();
      CreateMap<Entities.ApiKey, Models.ApiKeySecretDto>();
    }
  }
}