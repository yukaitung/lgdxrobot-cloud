using AutoMapper;
using Models = LGDXRobot2Cloud.Shared.Models;
using Entities = LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.UI.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      // Nodes
      CreateMap<Entities.ApiKey, Models.ApiKeyCreateDto>();
      CreateMap<Entities.ApiKey, Models.ApiKeyUpdateDto>();
    }
  }
}