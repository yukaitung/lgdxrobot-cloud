using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public interface IApiKeyLocationRepository
  {
    Task<ApiKeyLocation?> GetApiKeyLocationAsync(string location);
  }

  public class ApiKeyLocationRepository(LgdxContext context) : IApiKeyLocationRepository
  {
    private readonly LgdxContext _context = context;

    public async Task<ApiKeyLocation?> GetApiKeyLocationAsync(string location)
    {
      return await _context.ApiKeyLocations.Where(a => a.Name == location).FirstOrDefaultAsync();
    }
  }
}