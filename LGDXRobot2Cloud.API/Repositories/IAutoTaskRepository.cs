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

    Task<AutoTask?> GetFirstWaitingAutoTaskAsync(int robotId);
    Task<AutoTask?> GetOnGoingAutoTaskAsync(int robotId);
    Task<AutoTask?> GetAutoTaskToComplete(int robotId, int taskId, string token);
  }
}