using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface INodeService
  {
    Task<(IEnumerable<Node>?, PaginationMetadata?)> GetNodesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<Node?> GetNodeAsync(int nodeId);
    Task<Node?> AddNodeAsync(NodeCreateDto node);
    Task<bool> UpdateNodeAsync(int nodeId, NodeUpdateDto node);
    Task<bool> DeleteNodeAsync(int nodeId);
  }
}