using System.Security.Cryptography;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Administration;

public interface IApiKeyService
{
  Task<(IEnumerable<ApiKeyBusinessModel>, PaginationHelper)> GetApiKeysAsync(string? name, bool isThirdParty, int pageNumber, int pageSize);
  Task<ApiKeyBusinessModel> GetApiKeyAsync(int apiKeyId);
  Task<ApiKeySecretBusinessModel> GetApiKeySecretAsync(int apiKeyId);
  Task<ApiKeyBusinessModel> AddApiKeyAsync(ApiKeyCreateBusinessModel apiKeyCreateBusinessModel);
  Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateBusinessModel apiKeyUpdateBusinessModel);
  Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretUpdateBusinessModel apiKeySecretUpdateBusinessModel);
  Task<bool> DeleteApiKeyAsync(int apiKeyId);

  Task<IEnumerable<ApiKeySearchBusinessModel>> SearchApiKeysAsync(string? name);
  Task<int?> ValidateApiKeyAsync(string? apiKey);
}

public class ApiKeyService(
    IActivityLogService activityLogService,
    IMemoryCache memoryCache,
    LgdxContext context
  ) : IApiKeyService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  private void RemoveApiKeyCache(string apiKeySecret)
  {
    string hashed = LgdxHelper.GenerateSha256Hash(apiKeySecret);
    _memoryCache.Remove($"ValidateApiKeyAsync_{hashed}");
  }

  public async Task<(IEnumerable<ApiKeyBusinessModel>, PaginationHelper)> GetApiKeysAsync(string? name, bool isThirdParty, int pageNumber, int pageSize)
  {
    var query = _context.ApiKeys as IQueryable<ApiKey>;
    query = query.Where(a => a.IsThirdParty == isThirdParty);
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(a => a.Name.ToLower().Contains(name.ToLower()));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var apiKeys = await query.AsNoTracking()
      .OrderBy(a => a.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(a => new ApiKeyBusinessModel
      {
        Id = a.Id,
        Name = a.Name,
        IsThirdParty = a.IsThirdParty,
      })
      .ToListAsync();
    return (apiKeys, PaginationHelper);
  }

  public async Task<ApiKeyBusinessModel> GetApiKeyAsync(int apiKeyId)
  {
    return await _context.ApiKeys.AsNoTracking()
      .Where(a => a.Id == apiKeyId)
      .Select(a => new ApiKeyBusinessModel {
        Id = a.Id,
        Name = a.Name,
        IsThirdParty = a.IsThirdParty,
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();
  }

  public async Task<ApiKeySecretBusinessModel> GetApiKeySecretAsync(int apiKeyId)
  {
    var apiKeySecret = await _context.ApiKeys.AsNoTracking()
      .Where(a => a.Id == apiKeyId)
      .Select(a => new ApiKeySecretBusinessModel
      {
        Secret = a.Secret,
      })
      .FirstOrDefaultAsync()
        ?? throw new LgdxNotFound404Exception();

    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(ApiKey),
      EntityId = apiKeyId.ToString(),
      Action = ActivityAction.ApiKeySecretRead,
    });
      
    return apiKeySecret;
  }

  private static string GenerateApiKeys()
  {
    var bytes = RandomNumberGenerator.GetBytes(32);
    string base64String = Convert.ToBase64String(bytes)
      .Replace("+", "-")
      .Replace("/", "_");
    return "LGDX" + base64String;
  }

  public async Task<ApiKeyBusinessModel> AddApiKeyAsync(ApiKeyCreateBusinessModel apiKeyCreateBusinessModel)
  {
    if (!apiKeyCreateBusinessModel.IsThirdParty)
      apiKeyCreateBusinessModel.Secret = GenerateApiKeys();
    var apikey = apiKeyCreateBusinessModel.ToEntity();
    await _context.ApiKeys.AddAsync(apikey);
    await _context.SaveChangesAsync();
    
    await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
    {
      EntityName = nameof(ApiKey),
      EntityId = apikey.Id.ToString(),
      Action = ActivityAction.Create,
    });

    return new ApiKeyBusinessModel
    {
      Id = apikey.Id,
      Name = apikey.Name,
      IsThirdParty = apikey.IsThirdParty
    };
  }

  public async Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateBusinessModel apiKeyUpdateBusinessModel)
  {
    bool result = await _context.ApiKeys
      .Where(a => a.Id == apiKeyId)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(a => a.Name, apiKeyUpdateBusinessModel.Name)) == 1;

    if (result)
    {
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(ApiKey),
        EntityId = apiKeyId.ToString(),
        Action = ActivityAction.Update,
      });
    }
    return result;
  }

  public async Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretUpdateBusinessModel apiKeySecretUpdateBusinessModel)
  {
    var apiKey = await _context.ApiKeys.Where(a => a.Id == apiKeyId).FirstOrDefaultAsync()
      ?? throw new LgdxNotFound404Exception();
    if (!apiKey.IsThirdParty && !string.IsNullOrEmpty(apiKeySecretUpdateBusinessModel.Secret))
    {
      throw new LgdxValidation400Expection(nameof(apiKeySecretUpdateBusinessModel.Secret), "The LGDXRobot Cloud API Key cannot be changed.");
    }

    var oldSecret = apiKey.Secret;
    apiKey.Secret = apiKeySecretUpdateBusinessModel.Secret;
    bool result = await _context.SaveChangesAsync() == 1;

    if (result)
    {
      if (!apiKey.IsThirdParty)
      {
        RemoveApiKeyCache(oldSecret);
      }
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(ApiKey),
        EntityId = apiKeyId.ToString(),
        Action = ActivityAction.ApiKeySecretUpdate,
      });
    }
    return result;
  }

  public async Task<bool> DeleteApiKeyAsync(int apiKeyId)
  {
    var apiKey = await _context.ApiKeys.Where(a => a.Id == apiKeyId).FirstOrDefaultAsync()
      ?? throw new LgdxNotFound404Exception();
    
    var oldSecret = apiKey.Secret;
    bool result = await _context.ApiKeys.Where(a => a.Id == apiKeyId)
      .ExecuteDeleteAsync() == 1;

    if (result)
    {
      if (!apiKey.IsThirdParty)
      {
        RemoveApiKeyCache(oldSecret);
      }
      await _activityLogService.AddActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(ApiKey),
        EntityId = apiKeyId.ToString(),
        Action = ActivityAction.Delete,
      });
    }
    return result;
  }

  public async Task<IEnumerable<ApiKeySearchBusinessModel>> SearchApiKeysAsync(string? name)
  {
    var n = name ?? string.Empty;
    return await _context.ApiKeys.AsNoTracking()
      .Where(w => w.Name.ToLower().Contains(n.ToLower()))
      .Where(w => w.IsThirdParty == true)
      .Take(10)
      .Select(t => new ApiKeySearchBusinessModel {
        Id = t.Id,
        Name = t.Name,
      })
      .ToListAsync();
  }

  public async Task<int?> ValidateApiKeyAsync(string? apiKey)
  {
    if (string.IsNullOrWhiteSpace(apiKey))
    {
      return null;
    }

    string hashed = LgdxHelper.GenerateSha256Hash(apiKey);
    if (_memoryCache.TryGetValue($"ValidateApiKeyAsync_{hashed}", out int exist))
    {
      return exist;
    }
 
    int? apiKeyId = await _context.ApiKeys.AsNoTracking()
      .Where(a => a.Secret == apiKey)
      .Where(a => !a.IsThirdParty)
      .Select(a => a.Id)
      .FirstOrDefaultAsync();

    if (apiKeyId != null)
    {
      _memoryCache.Set($"ValidateApiKeyAsync_{hashed}", true, TimeSpan.FromMinutes(5));
    }

    return apiKeyId;
  }
}