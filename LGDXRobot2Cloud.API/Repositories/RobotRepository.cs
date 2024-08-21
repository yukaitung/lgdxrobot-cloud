using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface IRobotRepository
{
  Task<(IEnumerable<Robot>, PaginationHelper)> GetRobotsAsync(string? name, int pageNumber, int pageSize);
  Task<Robot?> GetRobotAsync(Guid robotId);
  Task<Robot?> GetRobotSimpleAsync(Guid robotId);
  Task AddRobotAsync(Robot robot);
  void DeleteRobot(Robot robot);
  Task<bool> SaveChangesAsync();
}

public class RobotRepository(LgdxContext context) : IRobotRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<Robot>, PaginationHelper)> GetRobotsAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Robots as IQueryable<Robot>;
    if (!string.IsNullOrEmpty(name))
    {
      name = name.Trim();
      query = query.Where(r => r.Name.Contains(name));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var robots = await query.OrderBy(r => r.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
    return (robots, PaginationHelper);
  }

  public async Task<Robot?> GetRobotAsync(Guid robotId)
  {
    return await _context.Robots.Where(r => r.Id == robotId)
      .Include(r => r.DefaultNodesCollection)
      .Include(r => r.RobotSystemInfo)
      .Include(r => r.RobotChassisInfo)
      .Include(r => r.AssignedTasks)
        .ThenInclude(t => t.Flow)
      .Include(r => r.AssignedTasks.Where(  t => t.CurrentProgressId != (int)ProgressState.Aborted 
                                              && t.CurrentProgressId != (int)ProgressState.Completed 
                                              && t.CurrentProgressId != (int)ProgressState.Template)
                                    .OrderByDescending(t => t.CurrentProgressId)
                                    .ThenBy(t => t.Id))
        .ThenInclude(t => t.CurrentProgress)
      .FirstOrDefaultAsync();
  }

  public async Task<Robot?> GetRobotSimpleAsync(Guid robotId)
  {
    return await _context.Robots.Where(r => r.Id == robotId)
      .FirstOrDefaultAsync();
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
