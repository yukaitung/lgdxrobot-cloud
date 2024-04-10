using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.Shared.Services;

namespace LGDXRobot2Cloud.UI.Services
{
  public class NodeService : INodeService
  {
    public readonly HttpClient _httpClient;
    public readonly JsonSerializerOptions _jsonSerializerOptions;

    public NodeService(HttpClient httpClient)
    {
      _httpClient = httpClient;
      _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
    }

    public async Task<(IEnumerable<Node>?, PaginationMetadata?)> GetNodesAsync(string? name, int pageNumber, int pageSize)
    {
      var url = name != null ? $"robot/nodes?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"robot/nodes?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      var paginationMetadataJson = response.Headers.Contains("X-Pagination") ? response.Headers.GetValues("X-Pagination").FirstOrDefault() : "";
      var paginationMetadata = JsonSerializer.Deserialize<PaginationMetadata>(paginationMetadataJson, _jsonSerializerOptions);
      var nodes = await JsonSerializer.DeserializeAsync<IEnumerable<Node>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (nodes, paginationMetadata);
    }

    public async Task<Node?> GetNodeAsync(int nodeId)
    {
      var response = await _httpClient.GetAsync($"robot/nodes/{nodeId}");
      var node = await JsonSerializer.DeserializeAsync<Node>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return node;
    }

    public async Task AddNodeAsync(NodeCreateDto node)
    {
      var nodeJson = new StringContent(JsonSerializer.Serialize(node), Encoding.UTF8, "application/json");
      await _httpClient.PostAsync("robot/nodes", nodeJson);
    }

    public async Task UpdateNodeAsync(int nodeId, NodeCreateDto node)
    {
      var nodeJson = new StringContent(JsonSerializer.Serialize(node), Encoding.UTF8, "application/json");
      await _httpClient.PutAsync($"robot/nodes/{nodeId}", nodeJson);
    }

    public async Task DeleteNodeAsync(int nodeId)
    {
      await _httpClient.DeleteAsync($"robot/nodes/{nodeId}");
    }
  }
}