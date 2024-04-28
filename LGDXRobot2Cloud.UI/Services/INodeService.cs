using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface INodeService
  {
    Task<(IEnumerable<NodeBlazor>?, PaginationMetadata?)> GetNodesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<NodeBlazor?> GetNodeAsync(int nodeId);
    Task<NodeBlazor?> AddNodeAsync(NodeCreateDto node);
    Task<bool> UpdateNodeAsync(int nodeId, NodeUpdateDto node);
    Task<bool> DeleteNodeAsync(int nodeId);
    Task<string> SearchNodesAsync(string name);
  }
}