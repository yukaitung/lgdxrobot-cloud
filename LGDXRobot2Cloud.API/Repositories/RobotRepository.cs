using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class RobotRepository : IRobotRepository
  {
    private readonly LgdxContext _context;

    public RobotRepository(LgdxContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<(IEnumerable<Robot>, PaginationMetadata)> GetRobotsAsync(string? name, int pageNumber, int pageSize)
    {
      var query = _context.Robots as IQueryable<Robot>;
      if (!string.IsNullOrEmpty(name))
      {
        name = name.Trim();
        query = query.Where(r => r.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageNumber, pageSize);
      var robots = await query.OrderBy(r => r.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (robots, paginationMetadata);
    }

    public async Task<Robot?> GetRobotAsync(int robotId)
    {
      return await _context.Robots.Where(r => r.Id == robotId).FirstOrDefaultAsync();
    }

    public async Task AddRobotAsync(Robot robot)
    {
      await _context.Robots.AddAsync(robot);
    }

    public void DeleteRobot(Robot robot)
    {
      _context.Robots.Remove(robot);
    }
    
    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }
  }
}