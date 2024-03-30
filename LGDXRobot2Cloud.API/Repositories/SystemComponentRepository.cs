using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Entities;
using LGDXRobot2Cloud.API.Services;
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

    public async Task<Dictionary<string, SystemComponent>> GetSystemComponentsInDictAsync()
    {
      var systemComponents = await _context.SystemComponents.ToListAsync();
      var result = new Dictionary<string, SystemComponent>();
      foreach(SystemComponent component in systemComponents)
      {
        result.Add(component.Name, component);
      }
      return result;
    }
  }
}
