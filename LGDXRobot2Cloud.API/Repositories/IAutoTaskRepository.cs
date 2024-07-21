using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IAutoTaskRepository
  {
    Task<(IEnumerable<AutoTask>, PaginationMetadata)> GetAutoTasksAsync(string? name, int? showProgressId, bool? showRunningTasks, int pageNumber, int pageSize);
    Task<AutoTask?> GetAutoTaskAsync(int autoTaskId);
    Task AddAutoTaskAsync(AutoTask autoTask);
    void DeleteAutoTask(AutoTask autoTask);
    Task<bool> SaveChangesAsync();

    Task<AutoTask?> AssignAutoTaskAsync(Guid robotId);
    Task<AutoTask?> GetRunningAutoTaskAsync(Guid robotId);
    Task<AutoTask?> AutoTaskNextAsync(Guid robotId, int taskId, string token);
    Task<AutoTask?> AutoTaskAbortAsync(Guid robotId, int taskId, string token);
  }
}