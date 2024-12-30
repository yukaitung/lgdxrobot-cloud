using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IProgressRepository
  {
    Task<(IEnumerable<Progress>, PaginationHelper)> GetProgressesAsync(string? name, int pageNumber, int pageSize, bool hideReserved, bool hideSystem);
    Task<Progress?> GetProgressAsync(int progressId);
    Task<bool> ProgressExistsAsync(int progressId);
    Task AddProgressAsync(Progress progress);
    void DeleteProgress(Progress progress);
    Task<bool> SaveChangesAsync();

    Task<IEnumerable<Progress>> SearchProgressesAsync(string name);
    Task<Dictionary<int, Progress>> GetProgressesDictFromListAsync(IEnumerable<int> progressIds);
  }
  
  public class ProgressRepository(LgdxContext context) : IProgressRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<Progress>, PaginationHelper)> GetProgressesAsync(string? name, int pageNumber, int pageSize, bool hideReserved, bool hideSystem)
    {
      var query = _context.Progresses as IQueryable<Progress>;
      if (!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name.Contains(name));
      }
      if (hideReserved)
      {
        query = query.Where(t => !t.Reserved);
      }
      if (hideSystem)
      {
        query = query.Where(t => !t.System);
      }
      var itemCount = await query.CountAsync();
      var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
      var progresses = await query.AsNoTracking()
        .OrderBy(a => a.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (progresses, PaginationHelper);
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

    public async Task<IEnumerable<Progress>> SearchProgressesAsync(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        return await _context.Progresses.AsNoTracking().Take(10).ToListAsync();
      }
      else
      {
        return await _context.Progresses.AsNoTracking().Where(w => w.Name.Contains(name)).Take(10).ToListAsync();
      }
    }

    public async Task<Dictionary<int, Progress>> GetProgressesDictFromListAsync(IEnumerable<int> progressIds)
    {
      return await _context.Progresses.Where(p => progressIds.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id, p => p);
    }
  }
}