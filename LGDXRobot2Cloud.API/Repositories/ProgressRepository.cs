using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class ProgressRepository : IProgressRepository
  {
    private readonly LgdxContext _context;

    public ProgressRepository(LgdxContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Progress>> GetProgressesAsync()
    {
      return await _context.Progresses.OrderBy(p => p.Id).ToListAsync();
    }
    
    public async Task<Progress?> GetProgressAsync(int progressId)
    {
      return await _context.Progresses.Where(p => p.Id == progressId).FirstOrDefaultAsync();
    }

    public async Task<bool> ProgressExistsAsync(int progressId)
    {
      return await _context.Progresses.AnyAsync(p => p.Id == progressId);
    }

    public async Task AddProgressAsync(Progress progress)
    {
      await _context.Progresses.AddAsync(progress);
    }

    public void DeleteProgress(Progress progress)
    {
      _context.Progresses.Remove(progress);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }
  }
}