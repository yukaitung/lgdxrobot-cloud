using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IRobotService
  {
    Task<(IEnumerable<RobotBlazor>?, PaginationMetadata?)> GetRobotsAsync(string? name = null, int pageNumber = 1, int pageSize = 16);
    Task<RobotBlazor?> GetRobotAsync(int robotId);
    Task<RobotBlazor?> AddRobotAsync(RobotCreateDto robot);
    Task<bool> DeleteRobotAsync(int robotId);
    Task<string> SearchRobotsAsync(string name);
  }
}