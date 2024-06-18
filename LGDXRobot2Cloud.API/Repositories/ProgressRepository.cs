using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class ProgressRepository(LgdxContext context) : IProgressRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<Progress>, PaginationMetadata)> GetProgressesAsync(string? name, int pageNumber, int pageSize, bool hideReserved)
    {
      var query = _context.Progresses as IQueryable<Progress>;
      if (!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name.Contains(name));
      }
      if (hideReserved)
      {
        query = query.Where(t => t.Reserved == false);
      }
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageNumber, pageSize);
      var progresses = await query.OrderBy(a => a.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (progresses, paginationMetadata);
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

    public async Task<Dictionary<int, Progress>> GetProgressesDictFromListAsync(IEnumerable<int> progressIds)
    {
      return await _context.Progresses.Where(p => progressIds.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id, p => p);
    }
  }
}