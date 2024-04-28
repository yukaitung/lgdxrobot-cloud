using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class AutoTaskRepository(LgdxContext context) : IAutoTaskRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<AutoTask>, PaginationMetadata)> GetAutoTasksAsync(string? name, int? showProgressId, bool? showRunningTasks, int pageNumber, int pageSize)
    {
      var query = _context.AutoTasks as IQueryable<AutoTask>;
      if (!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(t => t.Name != null && t.Name.Contains(name));
      }
       if (showProgressId != null) {
        query = query.Where(t => t.CurrentProgressId == showProgressId);
      }
      if (showRunningTasks == true)
        query = query.Where(t => (t.CurrentProgressId >= (int)ProgressState.Starting && t.CurrentProgressId <= (int)ProgressState.Completing) || t.CurrentProgressId > (int)ProgressState.Aborted);
      /*
      var predicate = PredicateBuilder.False<AutoTask>();
      if (showProgressId != null) {
        predicate = predicate.Or(t => t.CurrentProgressId == showProgressId);
      }
      if (showRunningTasks == true)
        predicate = predicate.Or(t => (t.CurrentProgressId >= (int)ProgressState.Starting && t.CurrentProgressId <= (int)ProgressState.Completing) || t.CurrentProgressId > (int)ProgressState.Aborted);
      query = query.Where(predicate);
      */
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageNumber, pageSize);
      var autoTasks = await query.OrderByDescending(t => t.Priority)
        .ThenBy(t => t.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .Include(t => t.Flow)
        .Include(t => t.AssignedRobot)
        .Include(t => t.CurrentProgress)
        .ToListAsync();
      return (autoTasks, paginationMetadata);
    }

    public async Task<AutoTask?> GetAutoTaskAsync(int autoTaskId)
    {
      return await _context.AutoTasks.Where(t => t.Id == autoTaskId)
        .Include(t => t.Details
          .OrderBy(td => td.Order))
        .ThenInclude(td => td.Waypoint)
        .Include(t => t.Flow)
        .Include(t => t.AssignedRobot)
        .Include(t => t.CurrentProgress)
        .FirstOrDefaultAsync();
    }

    public async Task AddAutoTaskAsync(AutoTask autoTask)
    {
      await _context.AutoTasks.AddAsync(autoTask);
    }

    public void DeleteAutoTask(AutoTask autoTask)
    {
      _context.AutoTasks.Remove(autoTask);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }
  }
}