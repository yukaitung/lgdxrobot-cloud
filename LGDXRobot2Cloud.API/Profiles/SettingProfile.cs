using AutoMapper;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      CreateMap<Entities.ApiKey, Models.ApiKeyDto>();
      CreateMap<Models.ApiKeyCreateDto, Entities.ApiKey>();
    }
  }
}