using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Services
{
  public interface IAutoTaskSchedulerService
  {
    Task<AutoTask?> GetAutoTask(Guid robotId);
    Task<string> AbortAutoTask(Guid robotId, int taskId, string token);
    Task<(AutoTask?, string)> CompleteProgress(Guid robotId, int taskId, string token);
  }
}