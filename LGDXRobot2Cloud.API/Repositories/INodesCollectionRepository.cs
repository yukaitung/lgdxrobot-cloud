using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface INodesCollectionRepository
  {
    Task<(IEnumerable<NodesCollection>, PaginationMetadata)> GetNodesCollectionsAsync(string? name, int pageNumber, int pageSize);
    Task<NodesCollection?> GetNodesCollectionAsync(int nodesCollectionId);
    Task AddNodesCollectionAsync(NodesCollection nodesCollection);
    void DeleteNodesCollection(NodesCollection nodesCollection);
    Task<bool> SaveChangesAsync();
  }
}