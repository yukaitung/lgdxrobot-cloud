using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;

namespace LGDXRobot2Cloud.UI.Services;

public interface IWaypointService
{
  Task<(IEnumerable<Waypoint>?, PaginationHelper?)> GetWaypointsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<Waypoint?> GetWaypointAsync(int waypointId);
  Task<bool> AddWaypointAsync(WaypointCreateDto waypoint);
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
  
  public async Task<(IEnumerable<Waypoint>?, PaginationHelper?)> GetWaypointsAsync(string? name, int pageNumber, int pageSize)
  {
    var url = name != null ? $"navigation/waypoints?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"navigation/waypoints?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var waypoints = await JsonSerializer.DeserializeAsync<IEnumerable<Waypoint>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (waypoints, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<Waypoint?> GetWaypointAsync(int waypointId)
  {
    var response = await _httpClient.GetAsync($"navigation/waypoints/{waypointId}");
    var waypoint = await JsonSerializer.DeserializeAsync<Waypoint>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return waypoint;
  }

  public async Task<bool> AddWaypointAsync(WaypointCreateDto waypoint)
  {
    var waypointJson = new StringContent(JsonSerializer.Serialize(waypoint), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/waypoints", waypointJson);
    return response.IsSuccessStatusCode;
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