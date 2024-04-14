using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface INodeService
  {
    Task<(IEnumerable<Node>?, PaginationMetadata?)> GetNodesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<NodeDto?> GetNodeAsync(int nodeId);
    Task<NodeDto?> AddNodeAsync(NodeCreateDto node);
    Task<bool> UpdateNodeAsync(int nodeId, NodeCreateDto node);
    Task<bool> DeleteNodeAsync(int nodeId);
  }
}