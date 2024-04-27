using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IFlowService
  {
    Task<(IEnumerable<FlowBlazor>?, PaginationMetadata?)> GetFlowsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<FlowBlazor?> GetFlowAsync(int flowId);
    Task<FlowBlazor?> AddFlowAsync(FlowCreateDto flow);
    Task<bool> UpdateFlowAsync(int flowId, FlowUpdateDto flow);
    Task<bool> DeleteFlowAsync(int flowId);
    Task<string> SearchFlowsAsync(string name);
  }
}