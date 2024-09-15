using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotService
{
  Task<(IEnumerable<Robot>?, PaginationHelper?)> GetRobotsAsync(string? name = null, int pageNumber = 1, int pageSize = 16);
  Task<Robot?> GetRobotAsync(string robotId);
  Task<RobotCreateResponseDto?> AddRobotAsync(RobotCreateDto robot);
  Task<bool> UpdateSoftwareEmergencyStop(string robotId, bool enable);
  Task<bool> UpdatePauseTaskAssigement(string robotId, bool enable);
  Task<bool> UpdateRobotInformationAsync(string robotId, RobotUpdateDto robot);
  Task<RobotCreateResponseDto?> RenewRobotCertificateAsync(string robotId, RobotRenewCertificateRenewDto dto);
  Task<bool> DeleteRobotAsync(string robotId);
  Task<string> SearchRobotsAsync(string name);
}
public sealed class RobotService : IRobotService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public RobotService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<Robot>?, PaginationHelper?)> GetRobotsAsync(string? name, int pageNumber, int pageSize)
  {
    var url = name != null ? $"robot?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"robot?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var robots = await JsonSerializer.DeserializeAsync<IEnumerable<Robot>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (robots, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<Robot?> GetRobotAsync(string robotId)
  {
    var response = await _httpClient.GetAsync($"robot/{robotId}");
    var robot = await JsonSerializer.DeserializeAsync<Robot>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
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

  public async Task<bool> UpdateSoftwareEmergencyStop(string robotId, bool enable)
  {
    EnableDto data = new() { Enable = enable };
    var dataJson = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync($"robot/{robotId}/emergencystop", dataJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdatePauseTaskAssigement(string robotId, bool enable)
  {
    EnableDto data = new() { Enable = enable };
    var dataJson = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync($"robot/{robotId}/pausetaskassigement", dataJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdateRobotInformationAsync(string robotId, RobotUpdateDto robot)
  {
    var robotJson = new StringContent(JsonSerializer.Serialize(robot), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync($"robot/{robotId}/information", robotJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<RobotCreateResponseDto?> RenewRobotCertificateAsync(string robotId, RobotRenewCertificateRenewDto dto)
  {
    var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync($"robot/{robotId}/certificate", json);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<RobotCreateResponseDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);;
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