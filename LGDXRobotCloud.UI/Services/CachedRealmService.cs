using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Services;

public interface ICachedRealmService
{
  Task<RealmDto> GetDefaultRealmAsync();
  Task<RealmDto> GetCurrrentRealmAsync(int realmId);
  void ClearCache(int realmId);

  string GetRealmName(int realmId);
}

public sealed class CachedRealmService (
    LgdxApiClient LgdxApiClient,
    IMemoryCache memoryCache,
    NavigationManager navigationManager
  ) : ICachedRealmService
{
  private readonly LgdxApiClient _lgdxApiClient = LgdxApiClient ?? throw new ArgumentNullException(nameof(LgdxApiClient));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
  private readonly NavigationManager _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

  private static RealmDto GetEmptyRealm()
  {
    return new RealmDto {
      Id = 0,
      Name = "Default",
      Description = "Default Realm",
      Image = "",
      Resolution = 0.0,
      OriginX = 0.0,
      OriginY = 0.0,
      OriginRotation = 0.0
    };
  }

  public async Task<RealmDto> GetDefaultRealmAsync()
  {
    if (_memoryCache.TryGetValue($"RealmService_GetDefaultRealm", out RealmDto? cachedMap))
    {
      return cachedMap ?? GetEmptyRealm();
    }

    try
    {
      var realm = await _lgdxApiClient.Navigation.Realms.Default.GetAsync();
      _memoryCache.Set($"RealmService_GetDefaultRealm", realm, _memoryCacheEntryOptions);
      return realm ?? GetEmptyRealm();
    }
    catch (ApiException ex)
    {
      if (ex.ResponseStatusCode == (int)System.Net.HttpStatusCode.Unauthorized)
        _navigationManager.NavigateTo(AppRoutes.Identity.Login + "?ReturnUrl=" + _navigationManager.ToBaseRelativePath(_navigationManager.Uri));
    }
    return GetEmptyRealm();
  }

  public async Task<RealmDto> GetCurrrentRealmAsync(int realmId)
  {
    if (realmId == 0)
    {
      return await GetDefaultRealmAsync();
    }
    if (_memoryCache.TryGetValue($"RealmService_GetCurrrentRealmAsync_{realmId}", out RealmDto? cachedMap))
    {
      return cachedMap ?? GetEmptyRealm();
    }

    try
    {
      var realm = await _lgdxApiClient.Navigation.Realms[realmId].GetAsync();
      _memoryCache.Set($"RealmService_GetCurrrentRealmAsync_{realmId}", realm, _memoryCacheEntryOptions);
      return realm ?? GetEmptyRealm();
    }
    catch (ApiException ex)
    {
      if (ex.ResponseStatusCode == (int)System.Net.HttpStatusCode.Unauthorized)
        _navigationManager.NavigateTo(AppRoutes.Identity.Login);
    }
    return GetEmptyRealm();
  }

  public string GetRealmName(int realmId)
  {
    if (realmId == 0)
    {
      if (_memoryCache.TryGetValue($"RealmService_GetDefaultRealm", out RealmDto? cachedMap))
      {
        return cachedMap?.Name ?? string.Empty;
      }
      return string.Empty;
    }

    if (_memoryCache.TryGetValue($"RealmService_GetCurrrentRealmAsync_{realmId}", out RealmDto? realm))
    {
      return realm?.Name ?? string.Empty;
    }
    else
    {
      return string.Empty;
    }
  }

  public void ClearCache(int realmId)
  {
    _memoryCache.Remove($"RealmService_GetCurrrentRealmAsync_{realmId}");
    var defaultRealm = _memoryCache.Get<RealmDto?>($"RealmService_GetDefaultRealm");
    if (defaultRealm != null && defaultRealm.Id == realmId)
    {
      _memoryCache.Remove($"RealmService_GetDefaultRealm");
    }
  }
}