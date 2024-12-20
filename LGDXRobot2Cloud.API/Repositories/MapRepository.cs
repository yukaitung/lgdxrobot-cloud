using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface IMapRepository
{
  Task<(IEnumerable<Map>, PaginationHelper)> GetMapsAsync(string? name, int pageNumber, int pageSize);
  Task<Map?> GetMapAsync(int id);
  Task AddMapAsync(Map map);
  void DeleteMap(Map map);
  Task<bool> SaveChangesAsync();
}

public class MapRepository(LgdxContext context) : IMapRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<Map>, PaginationHelper)> GetMapsAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Maps as IQueryable<Map>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(m => m.Name.Contains(name));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var maps = await query.AsNoTracking()
      .OrderBy(m => m.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
    return (maps, PaginationHelper);
  }

  public async Task<Map?> GetMapAsync(int id)
  {
    return await _context.Maps.Where(m => m.Id == id).FirstOrDefaultAsync();
  }

  public async Task AddMapAsync(Map map)
  {
    await _context.Maps.AddAsync(map);
  }

  public void DeleteMap(Map map)
  {
    _context.Maps.Remove(map);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync() >= 0;
  }
}