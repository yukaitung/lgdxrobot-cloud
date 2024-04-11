using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class TriggerRepository : ITriggerRepository
  {
    private readonly LgdxContext _context;

    public TriggerRepository(LgdxContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<(IEnumerable<Trigger>, PaginationMetadata)> GetTriggersAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.Triggers as IQueryable<Trigger>;
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageNumber, pageSize);
      var triggers = await query.OrderBy(t => t.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .Include(t => t.ApiKeyLocation)
        .Include(t => t.ApiKey)
        .ToListAsync();
      return (triggers, paginationMetadata);
    }

    public async Task<Trigger?> GetTriggerAsync(int triggerId)
    {
      return await _context.Triggers.Where(t => t.Id == triggerId)
        .Include(t => t.ApiKeyLocation)
        .Include(t => t.ApiKey)
        .FirstOrDefaultAsync();
    }

    public async Task AddTriggerAsync(Trigger trigger)
    {
      await _context.Triggers.AddAsync(trigger);
    }

    public void DeleteTrigger(Trigger trigger)
    {
      _context.Triggers.Remove(trigger);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }

    public async Task<Dictionary<int, Trigger>> GetTriggersDictFromListAsync(IEnumerable<int> triggerIds)
    {
      return await _context.Triggers.Where(p => triggerIds.Contains(p.Id))
        .Include(t => t.ApiKeyLocation)
        .Include(t => t.ApiKey)
        .ToDictionaryAsync(t => t.Id, t => t);
    }
  }
}