using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.Blazor;
using LGDXRobot2Cloud.Utilities.Helpers;

namespace LGDXRobot2Cloud.UI.Services;

public interface INodeService
{
  Task<(IEnumerable<NodeBlazor>?, PaginationHelper?)> GetNodesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<NodeBlazor?> GetNodeAsync(int nodeId);
  Task<NodeBlazor?> AddNodeAsync(NodeCreateDto node);
  Task<bool> UpdateNodeAsync(int nodeId, NodeUpdateDto node);
  Task<bool> DeleteNodeAsync(int nodeId);
  Task<string> SearchNodesAsync(string name);
}

public class NodeService : INodeService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public NodeService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<NodeBlazor>?, PaginationHelper?)> GetNodesAsync(string? name, int pageNumber, int pageSize)
  {
    var url = name != null ? $"robot/nodes?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"robot/nodes?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var nodes = await JsonSerializer.DeserializeAsync<IEnumerable<NodeBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (nodes, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<NodeBlazor?> GetNodeAsync(int nodeId)
  {
    var response = await _httpClient.GetAsync($"robot/nodes/{nodeId}");
    var node = await JsonSerializer.DeserializeAsync<NodeBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return node;
  }

  public async Task<NodeBlazor?> AddNodeAsync(NodeCreateDto node)
  {
    var nodeJson = new StringContent(JsonSerializer.Serialize(node), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("robot/nodes", nodeJson);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<NodeBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
      return null;
  }

  public async Task<bool> UpdateNodeAsync(int nodeId, NodeUpdateDto node)
  {
    var nodeJson = new StringContent(JsonSerializer.Serialize(node), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"robot/nodes/{nodeId}", nodeJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteNodeAsync(int nodeId)
  {
    var response = await _httpClient.DeleteAsync($"robot/nodes/{nodeId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchNodesAsync(string name)
  {
    var url = $"robot/nodes?name={name}";
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
