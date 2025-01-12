using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobot2Cloud.UI.Services;

public interface ICachedRealmService
{
  Task<RealmDto> GetDefaultRealmAsync();
  Task<RealmDto> GetCurrrentRealmAsync(int? realmId);
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
        _navigationManager.NavigateTo(AppRoutes.Identity.Login);
    }
    return GetEmptyRealm();
  }

  public async Task<RealmDto> GetCurrrentRealmAsync(int? realmId)
  {
    if (realmId == null)
    {
      return await GetDefaultRealmAsync();
    }
    if (_memoryCache.TryGetValue($"RealmService_GetCurrrentRealmAsync_{realmId}", out RealmDto? cachedMap))
    {
      return cachedMap ?? GetEmptyRealm();
    }

    try
    {
      var realm = await _lgdxApiClient.Navigation.Realms[(int)realmId].GetAsync();
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
}