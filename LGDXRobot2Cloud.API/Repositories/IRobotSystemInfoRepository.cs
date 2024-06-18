using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IRobotSystemInfoRepository
  {
    Task<RobotSystemInfo?> GetRobotSystemInfoAsync(Guid robotId);
    Task AddRobotSystemInfoAsync(RobotSystemInfo robot);
    Task<bool> SaveChangesAsync();
  }
}