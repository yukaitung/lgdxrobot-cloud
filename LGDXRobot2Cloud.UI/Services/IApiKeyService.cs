using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IApiKeyService
  {
    Task<(IEnumerable<ApiKeyBlazor>?, PaginationMetadata?)> GetApiKeysAsync(bool isThirdParty, string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<ApiKeyBlazor?> GetApiKeyAsync(int apiKeyId);
    Task<ApiKeyBlazor?> AddApiKeyAsync(ApiKeyCreateDto apiKey);
    Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateDto apiKey);
    Task<bool> DeleteApiKeyAsync(int apiKeyId);
    Task<ApiKeySecretBlazor?> GetApiKeySecretAsync(int apiKeyId);
    Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretDto apiKey);
    Task<string> SearchApiKeysAsync(string name);
  }
}