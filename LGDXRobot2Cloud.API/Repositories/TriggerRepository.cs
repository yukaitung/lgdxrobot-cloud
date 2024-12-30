using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface ITriggerRepository
  {
    Task<(IEnumerable<Trigger>, PaginationHelper)> GetTriggersAsync(string? name, int pageNumber, int pageSize);
    Task<Trigger?> GetTriggerAsync(int triggerId);
    Task AddTriggerAsync(Trigger trigger);
    void DeleteTrigger(Trigger trigger);
    Task<bool> SaveChangesAsync();

    Task<IEnumerable<Trigger>> SearchTriggersAsync(string name);
    Task<Dictionary<int, Trigger>> GetTriggersDictFromListAsync(IEnumerable<int> triggerIds);
  }
  
  public class TriggerRepository(LgdxContext context) : ITriggerRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<Trigger>, PaginationHelper)> GetTriggersAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.Triggers as IQueryable<Trigger>;
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
      var triggers = await query.AsNoTracking()
        .OrderBy(t => t.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (triggers, PaginationHelper);
    }

    public async Task<Trigger?> GetTriggerAsync(int triggerId)
    {
      return await _context.Triggers.Where(t => t.Id == triggerId)
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

    public async Task<IEnumerable<Trigger>> SearchTriggersAsync(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        return await _context.Triggers.AsNoTracking().Take(10).ToListAsync();
      }
      else
      {
        return await _context.Triggers.AsNoTracking().Where(w => w.Name.Contains(name)).Take(10).ToListAsync();
      }
    }

    public async Task<Dictionary<int, Trigger>> GetTriggersDictFromListAsync(IEnumerable<int> triggerIds)
    {
      return await _context.Triggers.Where(p => triggerIds.Contains(p.Id))
        .Include(t => t.ApiKey)
        .ToDictionaryAsync(t => t.Id, t => t);
    }
  }
}