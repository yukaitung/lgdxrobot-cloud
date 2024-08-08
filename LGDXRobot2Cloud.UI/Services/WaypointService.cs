using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services;

public interface IWaypointService
{
  Task<(IEnumerable<WaypointBlazor>?, PaginationMetadata?)> GetWaypointsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<WaypointBlazor?> GetWaypointAsync(int waypointId);
  Task<WaypointBlazor?> AddWaypointAsync(WaypointCreateDto waypoint);
  Task<bool> UpdateWaypointAsync(int waypointId, WaypointUpdateDto waypoint);
  Task<bool> DeleteWaypointAsync(int waypointId);
  Task<string> SearchWaypointsAsync(string name);
}

public class WaypointService : IWaypointService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public WaypointService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }
  
  public async Task<(IEnumerable<WaypointBlazor>?, PaginationMetadata?)> GetWaypointsAsync(string? name, int pageNumber, int pageSize)
  {
    var url = name != null ? $"navigation/waypoints?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"navigation/waypoints?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var paginationMetadataJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
      var waypoints = await JsonSerializer.DeserializeAsync<IEnumerable<WaypointBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (waypoints, paginationMetadata);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<WaypointBlazor?> GetWaypointAsync(int waypointId)
  {
    var response = await _httpClient.GetAsync($"navigation/waypoints/{waypointId}");
    var waypoint = await JsonSerializer.DeserializeAsync<WaypointBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return waypoint;
  }

  public async Task<WaypointBlazor?> AddWaypointAsync(WaypointCreateDto waypoint)
  {
    var waypointJson = new StringContent(JsonSerializer.Serialize(waypoint), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/waypoints", waypointJson);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<WaypointBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
      return null;
  }

  public async Task<bool> UpdateWaypointAsync(int waypointId, WaypointUpdateDto waypoint)
  {
    var waypointJson = new StringContent(JsonSerializer.Serialize(waypoint), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"navigation/waypoints/{waypointId}", waypointJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteWaypointAsync(int waypointId)
  {
    var response = await _httpClient.DeleteAsync($"navigation/waypoints/{waypointId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchWaypointsAsync(string name)
  {
    var url = $"navigation/waypoints?name={name}";
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
}