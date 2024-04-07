using LGDXRobot2Cloud.API.DbContexts;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.API.Services;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories
{
  public class ApiKeyRepository : IApiKeyRepository
  {
    private readonly LgdxContext _context;

    public ApiKeyRepository(LgdxContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<(IEnumerable<ApiKey>, PaginationMetadata)> GetApiKeysAsync(string? name, bool isThirdParty, int pageNumber, int pageSize)
    {
      var query = _context.ApiKeys as IQueryable<ApiKey>;
      query = query.Where(a => a.isThirdParty == isThirdParty);
      if(!string.IsNullOrWhiteSpace(name))
      {
        name = name.Trim();
        query = query.Where(a => a.Name.Contains(name));
      }
      var itemCount = await query.CountAsync();
      var paginationMetadata = new PaginationMetadata(itemCount, pageNumber, pageSize);
      var apiKeys = await query.OrderBy(a => a.Id)
        .Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();
      return (apiKeys, paginationMetadata);
    }

    public async Task<ApiKey?> GetApiKeyAsync(int apiKeyId)
    {
      return await _context.ApiKeys.Where(a => a.Id == apiKeyId).FirstOrDefaultAsync();
    }

    public async Task AddApiKeyAsync(ApiKey apiKey)
    {
      await _context.ApiKeys.AddAsync(apiKey);
    }

   public void DeleteApiKey(ApiKey apiKey)
    {
      _context.ApiKeys.Remove(apiKey);
    }

    public async Task<bool> SaveChangesAsync()
    {
      return await _context.SaveChangesAsync() >= 0;
    }
  }
}