using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IRobotTaskRepository
  {
    Task<(IEnumerable<RobotTask>, PaginationMetadata)> GetRobotTasksAsync(string? name, bool showWaiting, bool showProcessing, bool showCompleted, bool showAborted, int pageNumber, int pageSize);
    Task<RobotTask?> GetRobotTaskAsync(int robotTaskId);
    Task AddRobotTaskAsync(RobotTask robotTask);
    void DeleteRobotTask(RobotTask robotTask);
    Task<bool> SaveChangesAsync();
  }
}