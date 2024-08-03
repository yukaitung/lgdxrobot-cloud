using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class AutoTaskDetailRepository(LgdxContext context) : IAutoTaskDetailRepository
  {
    private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<AutoTaskDetail?> GetAutoTaskFirstDetailAsync(int taskId)
    {
      return await _context.AutoTasksDetail.Where(t => t.AutoTaskId == taskId)
        .Include(t => t.Waypoint)
        .OrderBy(t => t.Order)
        .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AutoTaskDetail>> GetAutoTaskDetailsAsync(int taskId)
    {
      return await _context.AutoTasksDetail.Where(t => t.AutoTaskId == taskId)
        .Include(t => t.Waypoint)
        .OrderBy(t => t.Order)
        .ToListAsync();
    }
  }
}