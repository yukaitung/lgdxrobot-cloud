using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;

namespace LGDXRobot2Cloud.UI.Services;

public interface INodesCollectionService
{
  Task<(IEnumerable<NodesCollection>?, PaginationHelper?)> GetNodesCollectionsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<NodesCollection?> GetNodesCollectionAsync(int nodesCollectionId);
  Task<bool> AddNodesCollectionAsync(NodesCollectionCreateDto nodesCollection);
  Task<bool> UpdateNodesCollectionAsync(int nodesCollectionId, NodesCollectionUpdateDto nodesCollection);
  Task<bool> DeleteNodesCollectionAsync(int nodesCollectionId);
}

public sealed class NodesCollectionService : INodesCollectionService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public NodesCollectionService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<NodesCollection>?, PaginationHelper?)> GetNodesCollectionsAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    var url = name != null ? $"robot/collections?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"robot/collections?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var nodesCollections = await JsonSerializer.DeserializeAsync<IEnumerable<NodesCollection>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (nodesCollections, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<NodesCollection?> GetNodesCollectionAsync(int nodesCollectionId)
  {
    var response = await _httpClient.GetAsync($"robot/collections/{nodesCollectionId}");
    var nodesCollection = await JsonSerializer.DeserializeAsync<NodesCollection>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return nodesCollection;
  }

  public async Task<bool> AddNodesCollectionAsync(NodesCollectionCreateDto nodesCollection)
  {
    var nodesCollectionJson = new StringContent(JsonSerializer.Serialize(nodesCollection), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("robot/collections", nodesCollectionJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdateNodesCollectionAsync(int nodesCollectionId, NodesCollectionUpdateDto nodesCollection)
  {
    var nodesCollectionJson = new StringContent(JsonSerializer.Serialize(nodesCollection), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"robot/collections/{nodesCollectionId}", nodesCollectionJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteNodesCollectionAsync(int nodesCollectionId)
  {
    var response = await _httpClient.DeleteAsync($"robot/collections/{nodesCollectionId}");
    return response.IsSuccessStatusCode;
  }
}
