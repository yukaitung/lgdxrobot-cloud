using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface IRealmRepository
{
  Task<(IEnumerable<Realm>, PaginationHelper)> GetRealmsAsync(string? name, int pageNumber, int pageSize);
  Task<Realm?> GetRealmAsync(int id);
  Task AddRealmAsync(Realm realm);
  void DeleteRealm(Realm realm);
  Task<bool> SaveChangesAsync();

  Task<Realm?> GetDefaultRealmAsync();
  Task<bool> IsRealmExistsAsync(int id); 
}

public class RealmRepository(LgdxContext context) : IRealmRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<Realm>, PaginationHelper)> GetRealmsAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Realms as IQueryable<Realm>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(m => m.Name.Contains(name));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var realms = await query.AsNoTracking()
      .OrderBy(m => m.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
    return (realms, PaginationHelper);
  }

  public async Task<Realm?> GetRealmAsync(int id)
  {
    return await _context.Realms.Where(m => m.Id == id).FirstOrDefaultAsync();
  }

  public async Task AddRealmAsync(Realm realm)
  {
    await _context.Realms.AddAsync(realm);
  }

  public void DeleteRealm(Realm realm)
  {
    _context.Realms.Remove(realm);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync() >= 0;
  }

  public async Task<Realm?> GetDefaultRealmAsync()
  {
    return await _context.Realms.OrderBy(m => m.Id).FirstOrDefaultAsync();
  }

  public async Task<bool> IsRealmExistsAsync(int id)
  {
    return await _context.Realms.AnyAsync(m => m.Id == id);
  }
}