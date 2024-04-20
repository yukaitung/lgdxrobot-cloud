using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public interface IProgressService
  {
    Task<(IEnumerable<ProgressBlazor>?, PaginationMetadata?)> GetProgressesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
    Task<ProgressBlazor?> GetProgressAsync(int progressId);
    Task<ProgressBlazor?> AddProgressAsync(ProgressCreateDto progress);
    Task<bool> UpdateProgressAsync(int progressId, ProgressUpdateDto progress);
    Task<bool> DeleteProgressAsync(int progressId);
  }
}