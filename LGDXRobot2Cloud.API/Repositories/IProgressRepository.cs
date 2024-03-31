using LGDXRobot2Cloud.API.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IProgressRepository
  {
    Task<IEnumerable<Progress>> GetProgressesAsync();
    Task<Progress?> GetProgressAsync(int progressId);
    Task<bool> ProgressExistsAsync(int progressId);
    Task AddProgressAsync(Progress progress);
    void DeleteProgress(Progress progress);
    Task<bool> SaveChangesAsync();

    // Specific Functions
    Task<Dictionary<int, Progress>> GetProgressesDictFromListAsync(IEnumerable<int> progressIds);
  }
}