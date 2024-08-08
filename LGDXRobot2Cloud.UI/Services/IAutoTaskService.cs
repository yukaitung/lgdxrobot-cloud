using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.Shared.Utilities;
using LGDXRobot2Cloud.Shared.Enums;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IAutoTaskService
  {
    Task<(IEnumerable<AutoTaskBlazor>?, PaginationMetadata?)> GetAutoTasksAsync(ProgressState? showProgressId = null, bool? showRunningTasks = null, string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<AutoTaskBlazor?> GetAutoTaskAsync(int autoTaskId);
    Task<AutoTaskBlazor?> AddAutoTaskAsync(AutoTaskCreateDto autoTask);
    Task<bool> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateDto autoTask);
    Task<bool> DeleteAutoTaskAsync(int autoTaskId);
  }
}