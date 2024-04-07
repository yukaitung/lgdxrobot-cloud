using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IApiKeyRepository
  {
    Task<(IEnumerable<ApiKey>, PaginationMetadata)> GetApiKeysAsync(string? name, bool isThirdParty, int pageNumber, int pageSize);
    Task<ApiKey?> GetApiKeyAsync(int apiKeyId);
    Task AddApiKeyAsync(ApiKey apiKey);
    void DeleteApiKey(ApiKey apiKey);
    Task<bool> SaveChangesAsync();
  }
}