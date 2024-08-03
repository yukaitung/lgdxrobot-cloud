using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IAutoTaskDetailRepository
  {
    Task<AutoTaskDetail?> GetAutoTaskFirstDetailAsync(int taskId);
    Task<IEnumerable<AutoTaskDetail>> GetAutoTaskDetailsAsync(int taskId);
  }
}