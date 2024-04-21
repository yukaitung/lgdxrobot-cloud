using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface ITriggerService
  {
    Task<(IEnumerable<TriggerBlazor>?, PaginationMetadata?)> GetTriggersAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<TriggerBlazor?> GetTriggerAsync(int triggerId);
    Task<TriggerBlazor?> AddTriggerAsync(TriggerCreateDto trigger);
    Task<bool> UpdateTriggerAsync(int triggerId, TriggerUpdateDto trigger);
    Task<bool> DeleteTriggerAsync(int triggerId);
  }
}