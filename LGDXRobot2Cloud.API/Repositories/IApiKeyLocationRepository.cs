using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IApiKeyLocationRepository
  {
    Task<ApiKeyLocation?> GetApiKeyLocationAsync(string location);
  }
}