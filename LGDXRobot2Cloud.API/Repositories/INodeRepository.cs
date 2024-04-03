using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface INodeRepository
  {
    Task<(IEnumerable<Node>, PaginationMetadata)> GetNodesAsync(string? name, int pageNumber, int pageSize);
    Task<Node?> GetNodeAsync(int nodeId);
    Task AddNodeAsync(Node node);
    void DeleteNode(Node node);
    Task<bool> SaveChangesAsync();
  }
}