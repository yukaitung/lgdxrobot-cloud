using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IAutoTaskDetailRepository
  {
    Task<IEnumerable<AutoTaskDetail>> GetAutoTaskDetailsAsync(int taskId);
  }
}