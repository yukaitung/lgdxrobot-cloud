using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Utilities.Helpers;
using LGDXRobot2Cloud.Utilities.Enums;
using LGDXRobot2Cloud.UI.Models;

namespace LGDXRobot2Cloud.UI.Services;

public interface IAutoTaskService
{
  Task<(IEnumerable<AutoTask>?, PaginationHelper?)> GetAutoTasksAsync(ProgressState? showProgressId = null, bool? showRunningTasks = null, string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<AutoTask?> GetAutoTaskAsync(int autoTaskId);
  Task<bool> AddAutoTaskAsync(AutoTaskCreateDto autoTask);
  Task<bool> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateDto autoTask);
  Task<bool> DeleteAutoTaskAsync(int autoTaskId);
  Task<bool> AbortAutoTaskAsync(int autoTaskId);
}

public sealed class AutoTaskService : IAutoTaskService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public AutoTaskService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<AutoTask>?, PaginationHelper?)> GetAutoTasksAsync(ProgressState? showProgressId = null, bool? showRunningTasks = null, string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    StringBuilder url = new($"navigation/tasks?pageNumber={pageNumber}&pageSize={pageSize}");
    if (showProgressId != null)
      url.Append($"&showProgressId={(int)showProgressId}");
    if (showRunningTasks == true)
      url.Append("&showRunningTasks=true");
    var response = await _httpClient.GetAsync(url.ToString());
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var tasks = await JsonSerializer.DeserializeAsync<IEnumerable<AutoTask>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (tasks, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }
  
  public async Task<AutoTask?> GetAutoTaskAsync(int autoTaskId)
  {
    var response = await _httpClient.GetAsync($"navigation/tasks/{autoTaskId}");
    var task = await JsonSerializer.DeserializeAsync<AutoTask>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return task;
  }

  public async Task<bool> AddAutoTaskAsync(AutoTaskCreateDto autoTask)
  {
    var taskJson = new StringContent(JsonSerializer.Serialize(autoTask), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/tasks", taskJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateDto autoTask)
  {
    var taskJson = new StringContent(JsonSerializer.Serialize(autoTask), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"navigation/tasks/{autoTaskId}", taskJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteAutoTaskAsync(int autoTaskId)
  {
    var response = await _httpClient.DeleteAsync($"navigation/tasks/{autoTaskId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> AbortAutoTaskAsync(int autoTaskId)
  {
    var response = await _httpClient.PostAsync($"navigation/tasks/{autoTaskId}/abort", null);
    return response.IsSuccessStatusCode;
  }
}
