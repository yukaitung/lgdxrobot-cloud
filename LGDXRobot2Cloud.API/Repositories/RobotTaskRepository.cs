using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;
using LGDXRobot2Cloud.API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class RobotTaskRepository : IRobotTaskRepository
  {
    private readonly LgdxContext _context;

    public RobotTaskRepository(LgdxContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<(IEnumerable<RobotTask>, PaginationMetadata)> GetRobotTasksAsync(string? name, bool showWaiting, bool showProcessing, bool showCompleted, bool showAborted, int pageNumber, int pageSize)
    {
      var query = _context.RobotTasks as IQueryable<RobotTask>;
      if (!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name == name);
      }
      query.Where(t => t.Name == name);
      var predicate = PredicateBuilder.False<RobotTask>();
      if (showWaiting)
        predicate = predicate.Or(t => t.CurrentProgressId == (int)ProgressState.Waiting);
      if (showProcessing)
        predicate = predicate.Or(t => (t.CurrentProgressId >= (int)ProgressState.Starting && t.CurrentProgressId <= (int)ProgressState.Completing) || t.CurrentProgressId > (int)ProgressState.Aborted);
      if (showCompleted)
        predicate = predicate.Or(t => t.CurrentProgressId == (int)ProgressState.Completed);
      if (showAborted)
        predicate = predicate.Or(t => t.CurrentProgressId == (int)ProgressState.Aborted);
      query.Where(predicate);
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageSize, pageNumber);
      var robotTasks = await query.OrderByDescending(t => t.Priority)
        .OrderBy(t => t.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .Include(t => t.CurrentProgress)
        .ToListAsync();
      return (robotTasks, paginationMetadata);
    }

    public async Task<RobotTask?> GetRobotTaskAsync(int robotTaskId)
    {
      return await _context.RobotTasks.Where(t => t.Id == robotTaskId)
        .Include(t => t.Waypoints)
        .ThenInclude(td => td.Waypoint)
        .Include(t => t.Flow)
        .Include(t => t.CurrentProgress)
        .FirstOrDefaultAsync();
    }

    public async Task AddRobotTaskAsync(RobotTask robotTask)
    {
      await _context.RobotTasks.AddAsync(robotTask);
    }

    public void DeleteRobotTask(RobotTask robotTask)
    {
      _context.RobotTasks.Remove(robotTask);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }
  }
}