using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services;

public interface IProgressService
{
  Task<(IEnumerable<ProgressBlazor>?, PaginationMetadata?)> GetProgressesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<ProgressBlazor?> GetProgressAsync(int progressId);
  Task<ProgressBlazor?> AddProgressAsync(ProgressCreateDto progress);
  Task<bool> UpdateProgressAsync(int progressId, ProgressUpdateDto progress);
  Task<bool> DeleteProgressAsync(int progressId);
  Task<string> SearchProgressesAsync(string name);
}

public class ProgressService : IProgressService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public ProgressService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<ProgressBlazor>?, PaginationMetadata?)> GetProgressesAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    var url = name != null ? $"navigation/progresses?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"navigation/progresses?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var paginationMetadataJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
      var progresses = await JsonSerializer.DeserializeAsync<IEnumerable<ProgressBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (progresses, paginationMetadata);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<ProgressBlazor?> GetProgressAsync(int progressId)
  {
    var response = await _httpClient.GetAsync($"navigation/progresses/{progressId}");
    var progress = await JsonSerializer.DeserializeAsync<ProgressBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return progress;
  }

  public async Task<ProgressBlazor?> AddProgressAsync(ProgressCreateDto progress)
  {
    var progressJson = new StringContent(JsonSerializer.Serialize(progress), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/progresses", progressJson);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<ProgressBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
      return null;
  }

  public async Task<bool> UpdateProgressAsync(int progressId, ProgressUpdateDto progress)
  {
    var progressJson = new StringContent(JsonSerializer.Serialize(progress), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"navigation/progresses/{progressId}", progressJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteProgressAsync(int progressId)
  {
    var response = await _httpClient.DeleteAsync($"navigation/progresses/{progressId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchProgressesAsync(string name)
  {
    var url = $"navigation/progresses?name={name}&hideReserved=true";
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