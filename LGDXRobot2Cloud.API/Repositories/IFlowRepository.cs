using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IFlowRepository
  {
    Task<(IEnumerable<Flow>, PaginationMetadata)> GetFlowsAsync(string? name, int pageNumber, int pageSize);
    Task<Flow?> GetFlowAsync(int flowId);
    Task AddFlowAsync(Flow flow);
    void DeleteFlow(Flow flow);
    Task<bool> SaveChangesAsync();
  }
}