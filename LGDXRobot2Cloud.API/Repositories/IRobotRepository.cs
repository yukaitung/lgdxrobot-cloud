using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IRobotRepository
  {
    Task<(IEnumerable<Robot>, PaginationMetadata)> GetRobotsAsync(string? name, int pageNumber, int pageSize);
    Task<Robot?> GetRobotAsync(Guid robotId);
    Task<Robot?> GetRobotSimpleAsync(Guid robotId);
    Task AddRobotAsync(Robot robot);
    void DeleteRobot(Robot robot);
    Task<bool> SaveChangesAsync();
  }
}