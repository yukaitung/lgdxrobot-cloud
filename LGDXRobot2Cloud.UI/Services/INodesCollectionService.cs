using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface INodesCollectionService
  {
    Task<(IEnumerable<NodesCollectionBlazor>?, PaginationMetadata?)> GetNodesCollectionsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<NodesCollectionBlazor?> GetNodesCollectionAsync(int nodesCollectionId);
    Task<NodesCollectionBlazor?> AddNodesCollectionAsync(NodesCollectionCreateDto nodesCollection);
    Task<bool> UpdateNodesCollectionAsync(int nodesCollectionId, NodesCollectionUpdateDto nodesCollection);
    Task<bool> DeleteNodesCollectionAsync(int nodesCollectionId);
  }
}