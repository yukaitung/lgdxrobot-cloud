using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IAutoTaskRepository
  {
    Task<(IEnumerable<AutoTask>, PaginationMetadata)> GetAutoTasksAsync(string? name, bool showWaiting, bool showProcessing, bool showCompleted, bool showAborted, int pageNumber, int pageSize);
    Task<AutoTask?> GetAutoTaskAsync(int autoTaskId);
    Task AddAutoTaskAsync(AutoTask autoTask);
    void DeleteAutoTask(AutoTask autoTask);
    Task<bool> SaveChangesAsync();
  }
}