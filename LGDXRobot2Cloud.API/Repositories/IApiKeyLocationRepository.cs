using LGDXRobot2Cloud.API.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IApiKeyLocationRepository
  {
    Task<ApiKeyLocation?> GetApiKeyLocationAsync(string location);
  }
}