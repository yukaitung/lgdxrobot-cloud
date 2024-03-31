using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface ITriggerRepository
  {
    Task<(IEnumerable<Trigger>, PaginationMetadata)> GetTriggersAsync(string? name, int pageNumber, int pageSize);
    Task<Trigger?> GetTriggerAsync(int triggerId);
    Task AddTriggerAsync(Trigger trigger);
    void DeleteTrigger(Trigger trigger);
    Task<bool> SaveChangesAsync();

    // Specific Functions
    Task<Dictionary<int, Trigger>> GetTriggersDictFromListAsync(IEnumerable<int> triggerIds);
  }
}