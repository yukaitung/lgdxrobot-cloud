using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IApiKeyService
  {
    Task<(IEnumerable<ApiKey>?, PaginationMetadata?)> GetApiKeysAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<ApiKey?> GetApiKeyAsync(int nodeId);
    Task<ApiKey?> AddApiKeyAsync(NodeCreateDto node);
    Task<bool> UpdateApiKeyAsync(int nodeId, NodeCreateDto node);
    Task<bool> DeleteApiKeyAsync(int nodeId);
  }
}