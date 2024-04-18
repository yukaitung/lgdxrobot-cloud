using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IApiKeyService
  {
    Task<(IEnumerable<ApiKey>?, PaginationMetadata?)> GetApiKeysAsync(bool isThirdParty, string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<ApiKey?> GetApiKeyAsync(int apiKeyId);
    Task<ApiKey?> AddApiKeyAsync(ApiKeyCreateDto apiKey);
    Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateDto apiKey);
    Task<bool> DeleteApiKeyAsync(int apiKeyId);
  }
}