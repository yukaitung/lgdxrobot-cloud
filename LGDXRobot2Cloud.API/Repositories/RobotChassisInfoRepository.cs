using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface IRobotChassisInfoRepository
{
  Task<RobotChassisInfo?> GetChassisInfoAsync(Guid robotId);
  Task AddChassisInfoAsync(RobotChassisInfo robot);
  Task<bool> SaveChangesAsync();
}

public class RobotChassisInfoRepository(LgdxContext context) : IRobotChassisInfoRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<RobotChassisInfo?> GetChassisInfoAsync(Guid robotId)
  {
    return await _context.RobotChassisInfos.Where(s => s.RobotId == robotId).FirstOrDefaultAsync();
  }

  public async Task AddChassisInfoAsync(RobotChassisInfo robot)
  {
    await _context.RobotChassisInfos.AddAsync(robot);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync() >= 0;
  }
}
