using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class ApiKeyLocationRepository : IApiKeyLocationRepository
  {
    private readonly LgdxContext _context;

    public ApiKeyLocationRepository(LgdxContext context)
    {
      _context = context;
    }

    public async Task<ApiKeyLocation?> GetApiKeyLocationAsync(string location)
    {
      return await _context.ApiKeyLocations.Where(a => a.Name == location).FirstOrDefaultAsync();
    }
  }
}