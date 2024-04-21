using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public class TriggerService : ITriggerService
  {
    public readonly HttpClient _httpClient;
    public readonly JsonSerializerOptions _jsonSerializerOptions;

    public TriggerService(HttpClient httpClient)
    {
      _httpClient = httpClient;
      _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
    }

    public async Task<(IEnumerable<TriggerBlazor>?, PaginationMetadata?)> GetTriggersAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
    {
      var url = name != null ? $"navigation/triggers?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"navigation/triggers?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var paginationMetadataJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
        var triggers = await JsonSerializer.DeserializeAsync<IEnumerable<TriggerBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return (triggers, paginationMetadata);
      }
      else
      {
        throw new Exception($"The API service returns status code {response.StatusCode}.");
      }
    }

    public async Task<TriggerBlazor?> GetTriggerAsync(int triggerId)
    {
      var response = await _httpClient.GetAsync($"navigation/triggers/{triggerId}");
      var trigger = await JsonSerializer.DeserializeAsync<TriggerBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return trigger;
    }

    public async Task<TriggerBlazor?> AddTriggerAsync(TriggerCreateDto trigger)
    {
      var triggerJson = new StringContent(JsonSerializer.Serialize(trigger), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("navigation/triggers", triggerJson);
      if (response.IsSuccessStatusCode)
      {
        return await JsonSerializer.DeserializeAsync<TriggerBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      }
      else
        return null;
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
  }
}