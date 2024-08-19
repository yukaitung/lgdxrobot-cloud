using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface IRobotSystemInfoRepository
{
  Task<RobotSystemInfo?> GetRobotSystemInfoAsync(Guid robotId);
  Task AddRobotSystemInfoAsync(RobotSystemInfo robot);
  Task<bool> SaveChangesAsync();
}

public class RobotSystemInfoRepository(LgdxContext context) : IRobotSystemInfoRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<RobotSystemInfo?> GetRobotSystemInfoAsync(Guid robotId)
  {
    return await _context.RobotSystemInfos.Where(s => s.RobotId == robotId).FirstOrDefaultAsync();
  }

  public async Task AddRobotSystemInfoAsync(RobotSystemInfo robot)
  {
    await _context.RobotSystemInfos.AddAsync(robot);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync() >= 0;
  }
}
