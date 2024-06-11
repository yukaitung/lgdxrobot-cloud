using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.Services
{
  public interface IAutoTaskSchedulerService
  {
    Task<AutoTask?> GetAutoTask(int robotId);
    Task<string> AbortAutoTask(int robotId, int taskId, string token);
    Task<(AutoTask?, string)> CompleteProgress(int robotId, int taskId, string token);
  }
}