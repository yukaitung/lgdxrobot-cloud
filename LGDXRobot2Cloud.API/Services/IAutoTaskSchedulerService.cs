using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Services
{
  public interface IAutoTaskSchedulerService
  {
    Task<AutoTask?> GetAutoTask(Guid robotId);
    Task<string> AutoTaskAbort(Guid robotId, int taskId, string token);
    Task<(AutoTask?, string)> AutoTaskNext(Guid robotId, int taskId, string token);
  }
}