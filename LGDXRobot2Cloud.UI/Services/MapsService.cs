using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.UI.Services;

public interface IMapsService
{
  Task<(IEnumerable<Map>?, PaginationHelper?)> GetMapsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<Map?> GetMapAsync(int mapId);
  Task<bool> AddMapAsync(MapCreateDto map);
  Task<bool> UpdateMapAsync(int mapId, MapUpdateDto map);
  Task<bool> DeleteMapAsync(int mapId);
  Task<string> SearchMapsAsync(string name);

  Task<Map?> GetDefaultMapAsync();
}

public sealed class MapsService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient,
    IMemoryCache memoryCache
  ) : BaseService(authenticationStateProvider, httpClient), IMapsService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  public async Task<(IEnumerable<Map>?, PaginationHelper?)> GetMapsAsync(string? name, int pageNumber, int pageSize)
  {
    var url = name != null ? $"navigation/maps?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"navigation/maps?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var maps = await JsonSerializer.DeserializeAsync<IEnumerable<Map>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (maps, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<Map?> GetMapAsync(int mapId)
  {
    var response = await _httpClient.GetAsync($"navigation/maps/{mapId}");
    var map = await JsonSerializer.DeserializeAsync<Map>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return map;
  }

  public async Task<bool> AddMapAsync(MapCreateDto map)
  {
    var mapJson = new StringContent(JsonSerializer.Serialize(map), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/maps", mapJson);
    return response.IsSuccessStatusCode;
  } 

  public async Task<bool> UpdateMapAsync(int mapId, MapUpdateDto map)
  {
    var mapJson = new StringContent(JsonSerializer.Serialize(map), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"navigation/maps/{mapId}", mapJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteMapAsync(int mapId)
  {
    var response = await _httpClient.DeleteAsync($"navigation/maps/{mapId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchMapsAsync(string name)
  {
    var url = $"navigation/maps?name={name}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      return await response.Content.ReadAsStringAsync();
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<Map?> GetDefaultMapAsync()
  {
    if (_memoryCache.TryGetValue($"MapsService_GetDefaultMap", out Map? cachedMap))
    {
      return cachedMap;
    }
    else
    {
      var response = await _httpClient.GetAsync("navigation/maps/default");
      var map = await JsonSerializer.DeserializeAsync<Map>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      _memoryCache.Set($"MapsService_GetDefaultMap", map);
      return map;
    }
  }
}