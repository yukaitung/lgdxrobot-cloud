using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotService
{
  Task<(IEnumerable<RobotBlazor>?, PaginationMetadata?)> GetRobotsAsync(string? name = null, int pageNumber = 1, int pageSize = 16);
  Task<RobotBlazor?> GetRobotAsync(string robotId);
  Task<RobotCreateResponseDto?> AddRobotAsync(RobotCreateDto robot);
  Task<bool> DeleteRobotAsync(string robotId);
  Task<string> SearchRobotsAsync(string name);
}
public class RobotService : IRobotService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public RobotService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<RobotBlazor>?, PaginationMetadata?)> GetRobotsAsync(string? name, int pageNumber, int pageSize)
  {
    var url = name != null ? $"robot?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"robot?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var paginationMetadataJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
      var robots = await JsonSerializer.DeserializeAsync<IEnumerable<RobotBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (robots, paginationMetadata);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<RobotBlazor?> GetRobotAsync(string robotId)
  {
    var response = await _httpClient.GetAsync($"robot/{robotId}");
    var robot = await JsonSerializer.DeserializeAsync<RobotBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return robot;
  }
  
  public async Task<RobotCreateResponseDto?> AddRobotAsync(RobotCreateDto robot)
  {
    var robotJson = new StringContent(JsonSerializer.Serialize(robot), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("robot", robotJson);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<RobotCreateResponseDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
      return null;
  }

  public async Task<bool> DeleteRobotAsync(string robotId)
  {
    var response = await _httpClient.DeleteAsync($"robot/{robotId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchRobotsAsync(string name)
  {
    var url = $"robot?name={name}";
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