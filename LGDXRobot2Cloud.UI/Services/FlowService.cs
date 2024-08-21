using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.Blazor;
using LGDXRobot2Cloud.Utilities.Helpers;

namespace LGDXRobot2Cloud.UI.Services;

public interface IFlowService
{
  Task<(IEnumerable<FlowBlazor>?, PaginationHelper?)> GetFlowsAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<FlowBlazor?> GetFlowAsync(int flowId);
  Task<FlowBlazor?> AddFlowAsync(FlowCreateDto flow);
  Task<bool> UpdateFlowAsync(int flowId, FlowUpdateDto flow);
  Task<bool> DeleteFlowAsync(int flowId);
  Task<string> SearchFlowsAsync(string name);
}

public class FlowService : IFlowService
{
  public readonly HttpClient _httpClient;
  public readonly JsonSerializerOptions _jsonSerializerOptions;

  public FlowService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<(IEnumerable<FlowBlazor>?, PaginationHelper?)> GetFlowsAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    var url = name != null ? $"navigation/flows?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"navigation/flows?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var flows = await JsonSerializer.DeserializeAsync<IEnumerable<FlowBlazor>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (flows, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<FlowBlazor?> GetFlowAsync(int flowId)
  {
    var response = await _httpClient.GetAsync($"navigation/flows/{flowId}");
    var flow = await JsonSerializer.DeserializeAsync<FlowBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return flow;
  }

  public async Task<FlowBlazor?> AddFlowAsync(FlowCreateDto flow)
  {
    var flowJson = new StringContent(JsonSerializer.Serialize(flow), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("navigation/flows", flowJson);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<FlowBlazor>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
      return null;
  }
  
  public async Task<bool> UpdateFlowAsync(int flowId, FlowUpdateDto flow)
  {
    var flowJson = new StringContent(JsonSerializer.Serialize(flow), Encoding.UTF8, "application/json");
    Console.WriteLine(flowJson.ReadAsStringAsync().Result);
    var response = await _httpClient.PutAsync($"navigation/flows/{flowId}", flowJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteFlowAsync(int flowId)
  {
    var response = await _httpClient.DeleteAsync($"navigation/flows/{flowId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchFlowsAsync(string name)
  {
    var url = $"navigation/flows?name={name}";
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
