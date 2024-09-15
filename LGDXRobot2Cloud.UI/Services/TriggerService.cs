using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Utilities.Helpers;
using LGDXRobot2Cloud.UI.Models;

namespace LGDXRobot2Cloud.UI.Services;

public interface ITriggerService
{
  Task<(IEnumerable<Trigger>?, PaginationHelper?)> GetTriggersAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<Trigger?> GetTriggerAsync(int triggerId);
  Task<bool> AddTriggerAsync(TriggerCreateDto trigger);
  Task<bool> UpdateTriggerAsync(int triggerId, TriggerUpdateDto trigger);
  Task<bool> DeleteTriggerAsync(int triggerId);
  Task<string> SearchTriggersAsync(string name);
}

public class TriggerService : ITriggerService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public TriggerService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<Trigger>?, PaginationHelper?)> GetTriggersAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    var url = name != null ? $"navigation/triggers?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"navigation/triggers?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var triggers = await JsonSerializer.DeserializeAsync<IEnumerable<Trigger>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (triggers, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<Trigger?> GetTriggerAsync(int triggerId)
  {
    var response = await _httpClient.GetAsync($"navigation/triggers/{triggerId}");
    var trigger = await JsonSerializer.DeserializeAsync<Trigger>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return trigger;
  }

  public async Task<bool> AddTriggerAsync(TriggerCreateDto trigger)
  {
    var triggerJson = new StringContent(JsonSerializer.Serialize(trigger), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/triggers", triggerJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdateTriggerAsync(int triggerId, TriggerUpdateDto trigger)
  {
    var triggerJson = new StringContent(JsonSerializer.Serialize(trigger), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"navigation/triggers/{triggerId}", triggerJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteTriggerAsync(int triggerId)
  {
    var response = await _httpClient.DeleteAsync($"navigation/triggers/{triggerId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchTriggersAsync(string name)
  {
    var url = $"navigation/triggers?name={name}";
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