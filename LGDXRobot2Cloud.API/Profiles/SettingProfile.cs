using AutoMapper;
using Entities = LGDXRobot2Cloud.Data.Entities;
using Models = LGDXRobot2Cloud.Data.Models.DTOs;

namespace LGDXRobot2Cloud.API.Profiles
{
  public class SettingProfile : Profile
  {
    public SettingProfile()
    {
      CreateMap<Entities.ApiKey, Models.Responses.ApiKeyDto>();
      CreateMap<Models.Commands.ApiKeyCreateDto, Entities.ApiKey>();
      CreateMap<Models.Commands.ApiKeyUpdateDto, Entities.ApiKey>();
      CreateMap<Models.Responses.ApiKeySecretDto, Entities.ApiKey>();
      CreateMap<Entities.ApiKey, Models.Responses.ApiKeySecretDto>();
    }
  }
}