using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class SystemComponentRepository : ISystemComponentRepository
  {
    private readonly LgdxContext _context;

    public SystemComponentRepository(LgdxContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Dictionary<string, SystemComponent>> GetSystemComponentsDictAsync()
    {
      return await _context.SystemComponents.ToDictionaryAsync(s => s.Name, s => s);
    }
  }
}
