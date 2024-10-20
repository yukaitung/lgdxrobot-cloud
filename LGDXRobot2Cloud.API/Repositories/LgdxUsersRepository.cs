using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface ILgdxUsersRepository
{
  Task<(IEnumerable<LgdxUser>, PaginationHelper)> GetUsersAsync(string? name, int pageNumber, int pageSize);
  Task<bool> SaveChangesAsync();
}

public class LgdxUsersRepository(LgdxContext context) : ILgdxUsersRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<LgdxUser>, PaginationHelper)> GetUsersAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Users as IQueryable<LgdxUser>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(u => u.UserName!.Contains(name));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var users = await query
      .OrderBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
    return (users, PaginationHelper);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync() >= 0;
  }
}