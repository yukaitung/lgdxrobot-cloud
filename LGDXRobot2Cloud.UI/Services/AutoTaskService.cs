using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.Shared.Enums;

namespace LGDXRobot2Cloud.UI.Services;

public interface IAutoTaskService
{
  Task<(IEnumerable<AutoTaskBlazor>?, PaginationMetadata?)> GetAutoTasksAsync(ProgressState? showProgressId = null, bool? showRunningTasks = null, string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<AutoTaskBlazor?> GetAutoTaskAsync(int autoTaskId);
  Task<AutoTaskBlazor?> AddAutoTaskAsync(AutoTaskCreateDto autoTask);
  Task<bool> UpdateAutoTaskAsync(int autoTaskId, AutoTaskUpdateDto autoTask);
  Task<bool> DeleteAutoTaskAsync(int autoTaskId);
}

public class AutoTaskService : IAutoTaskService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public AutoTaskService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<AutoTaskBlazor>?, PaginationMetadata?)> GetAutoTasksAsync(ProgressState? showProgressId = null, bool? showRunningTasks = null, string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    StringBuilder url = new($"navigation/tasks?pageNumber={pageNumber}&pageSize={pageSize}");
    if (showProgressId != null)
      url.Append($"&showProgressId={(int)showProgressId}");
    if (showRunningTasks == true)
      url.Append("&showRunningTasks=true");
    var response = await _httpClient.GetAsync(url.ToString());
    if (response.IsSuccessStatusCode)
    {
      var paginationMetadataJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
      var tasks = await JsonSerializer.DeserializeAsync<IEnumerable<AutoTaskBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (tasks, paginationMetadata);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }
  
  public async Task<AutoTaskBlazor?> GetAutoTaskAsync(int autoTaskId)
  {
    var response = await _httpClient.GetAsync($"navigation/tasks/{autoTaskId}");
    var task = await JsonSerializer.DeserializeAsync<AutoTaskBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return task;
  }

  public async Task<AutoTaskBlazor?> AddAutoTaskAsync(AutoTaskCreateDto autoTask)
  {
    var taskJson = new StringContent(JsonSerializer.Serialize(autoTask), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/tasks", taskJson);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<AutoTaskBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
      return null;
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
}
