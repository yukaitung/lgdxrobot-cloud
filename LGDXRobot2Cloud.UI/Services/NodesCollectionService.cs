using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public class NodesCollectionService : INodesCollectionService
  {
    public readonly HttpClient _httpClient;
    public readonly JsonSerializerOptions _jsonSerializerOptions;

    public NodesCollectionService(HttpClient httpClient)
    {
      _httpClient = httpClient;
      _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
    }

    public async Task<(IEnumerable<NodesCollectionBlazor>?, PaginationMetadata?)> GetNodesCollectionsAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
    {
      var url = name != null ? $"robot/collections?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"robot/collections?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var paginationMetadataJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
        var nodesCollections = await JsonSerializer.DeserializeAsync<IEnumerable<NodesCollectionBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return (nodesCollections, paginationMetadata);
      }
      else
      {
        throw new Exception($"The API service returns status code {response.StatusCode}.");
      }
    }

    public async Task<NodesCollectionBlazor?> GetNodesCollectionAsync(int nodesCollectionId)
    {
      var response = await _httpClient.GetAsync($"robot/collections/{nodesCollectionId}");
      var nodesCollection = await JsonSerializer.DeserializeAsync<NodesCollectionBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return nodesCollection;
    }

    public async Task<NodesCollectionBlazor?> AddNodesCollectionAsync(NodesCollectionCreateDto nodesCollection)
    {
      var nodesCollectionJson = new StringContent(JsonSerializer.Serialize(nodesCollection), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("robot/collections", nodesCollectionJson);
      if (response.IsSuccessStatusCode)
      {
        return await JsonSerializer.DeserializeAsync<NodesCollectionBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      }
      else
        return null;
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
}