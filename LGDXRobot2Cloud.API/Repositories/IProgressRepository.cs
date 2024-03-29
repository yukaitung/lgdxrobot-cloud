using LGDXRobot2Cloud.API.Entities;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IProgressRepository
  {
    Task<IEnumerable<Progress>> GetProgressesAsync();
    Task<Progress?> GetProgressAsync(int ProgressId);
    Task<bool> ProgressExistsAsync(int ProgressId);
    Task AddProgressAsync(Progress Progress);
    void DeleteProgress(Progress Progress);
    Task<bool> SaveChangesAsync();
  }
}