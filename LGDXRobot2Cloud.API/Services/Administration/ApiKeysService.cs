using System.Security.Cryptography;
using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Administration;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Administration;

public interface IApiKeyService
{
  Task<(IEnumerable<ApiKeyBusinessModel>, PaginationHelper)> GetApiKeysAsync(string? name, bool isThirdParty, int pageNumber, int pageSize);
  Task<ApiKeyBusinessModel?> GetApiKeyAsync(int apiKeyId);
  Task<ApiKeySecretBusinessModel?> GetApiKeySecretAsync(int apiKeyId);
  Task<ApiKeyBusinessModel> AddApiKeyAsync(ApiKeyCreateBusinessModel apiKeyCreateBusinessModel);
  Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateBusinessModel apiKeyUpdateBusinessModel);
  Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretUpdateBusinessModel apiKeySecretUpdateBusinessModel);
  Task<bool> DeleteApiKeyAsync(int apiKeyId);

  Task<IEnumerable<ApiKeySearchBusinessModel>> SearchApiKeysAsync(string? name);
}

public class ApiKeyService(LgdxContext context) : IApiKeyService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<ApiKeyBusinessModel>, PaginationHelper)>GetApiKeysAsync(string? name, bool isThirdParty, int pageNumber, int pageSize)
  {
    var query = _context.ApiKeys as IQueryable<ApiKey>;
    query = query.Where(a => a.IsThirdParty == isThirdParty);
    if(!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim();
      query = query.Where(a => a.Name.Contains(name));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var apiKeys = await query.AsNoTracking()
      .OrderBy(a => a.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(a => new ApiKeyBusinessModel {
        Id = a.Id,
        Name = a.Name,
        IsThirdParty = a.IsThirdParty,
      })
      .ToListAsync();
    return (apiKeys, PaginationHelper);
  }

  public async Task<ApiKeyBusinessModel?> GetApiKeyAsync(int apiKeyId)
  {
    return await _context.ApiKeys.AsNoTracking()
      .Where(a => a.Id == apiKeyId)
      .Select(a => new ApiKeyBusinessModel {
        Id = a.Id,
        Name = a.Name,
        IsThirdParty = a.IsThirdParty,
      })
      .FirstOrDefaultAsync();
  }

  public async Task<ApiKeySecretBusinessModel?> GetApiKeySecretAsync(int apiKeyId)
  {
    return await _context.ApiKeys.AsNoTracking()
      .Where(a => a.Id == apiKeyId)
      .Select(a => new ApiKeySecretBusinessModel {
        Secret = a.Secret,
      })
      .FirstOrDefaultAsync();
  }

  private static string GenerateApiKeys()
  {
    var bytes = RandomNumberGenerator.GetBytes(32);
    string base64String = Convert.ToBase64String(bytes)
      .Replace("+", "-")
      .Replace("/", "_");
    return "LGDX2" + base64String;
  }

  public async Task<ApiKeyBusinessModel> AddApiKeyAsync(ApiKeyCreateBusinessModel apiKeyCreateBusinessModel)
  {
    apiKeyCreateBusinessModel.Secret ??= GenerateApiKeys();
    var apikey = apiKeyCreateBusinessModel.ToEntity();
    await _context.ApiKeys.AddAsync(apikey);
    await _context.SaveChangesAsync();
    return new ApiKeyBusinessModel{
      Id = apikey.Id,
      Name = apikey.Name,
      IsThirdParty = apikey.IsThirdParty
    };
  }

  public async Task<bool> UpdateApiKeyAsync(int apiKeyId, ApiKeyUpdateBusinessModel apiKeyUpdateBusinessModel)
  {
    return await _context.ApiKeys
      .Where(a => a.Id == apiKeyId)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(a => a.Name, apiKeyUpdateBusinessModel.Name)) == 1;
  }

  public async Task<bool> UpdateApiKeySecretAsync(int apiKeyId, ApiKeySecretUpdateBusinessModel apiKeySecretUpdateBusinessModel)
  {
    var apiKey = await _context.ApiKeys.Where(a => a.Id == apiKeyId).FirstOrDefaultAsync() 
      ?? throw new LgdxNotFound404Exception();
    if (!apiKey.IsThirdParty && !string.IsNullOrEmpty(apiKeySecretUpdateBusinessModel.Secret))
    {
      throw new LgdxValidation400Expection(nameof(apiKeySecretUpdateBusinessModel.Secret), "The LGDXRobot2 API Key cannot be changed.");
    }

    apiKey.Secret = apiKeySecretUpdateBusinessModel.Secret;
    await _context.SaveChangesAsync();
    return true;
  }

    public async Task<bool> DeleteApiKeyAsync(int apiKeyId)
  {
    return await _context.ApiKeys.Where(a => a.Id == apiKeyId)
      .ExecuteDeleteAsync() == 1;
  }

  public async Task<IEnumerable<ApiKeySearchBusinessModel>> SearchApiKeysAsync(string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return await _context.ApiKeys.AsNoTracking()
      .Take(10)
      .Select(a => new ApiKeySearchBusinessModel{
        Id = a.Id,
        Name = a.Name
      })
      .ToListAsync();
    }
    else
    {
      return await _context.ApiKeys.AsNoTracking()
        .Where(w => w.Name.Contains(name))
        .Take(10)
        .Select(a => new ApiKeySearchBusinessModel{
          Id = a.Id,
          Name = a.Name
        })
        .ToListAsync();
    }
  }
}
