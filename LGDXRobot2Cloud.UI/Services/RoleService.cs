using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRoleService
{
  Task<(IEnumerable<LgdxRole>?, PaginationHelper?)> GetRolesAsync(string? name = null, int pageNumber = 1, int pageSize = 10);
  Task<LgdxRole?> GetRoleAsync(string id);
  Task<bool> AddRoleAsync(LgdxRoleCreateDto role);
  Task<bool> UpdateRoleAsync(string id, LgdxRoleUpdateDto role);
  Task<bool> DeleteRoleAsync(string id);
  Task<string> SearchRolesAsync(string name);
}

public sealed class RoleService(
  AuthenticationStateProvider authenticationStateProvider, 
  HttpClient httpClient) : BaseService(authenticationStateProvider, httpClient), IRoleService
{
  public async Task<(IEnumerable<LgdxRole>?, PaginationHelper?)> GetRolesAsync(string? name = null, int pageNumber = 1, int pageSize = 10)
  {
    var url = name != null ? $"Identity/Roles?name={name}&pageNumber={pageNumber}&pageSize={pageSize}" : $"Identity/Roles?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var roles = await JsonSerializer.DeserializeAsync<IEnumerable<LgdxRole>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (roles, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<LgdxRole?> GetRoleAsync(string id)
  {
    var response = await _httpClient.GetAsync($"Identity/Roles/{id}");
    var role = await JsonSerializer.DeserializeAsync<LgdxRole>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return role;
  }

  public async Task<bool> AddRoleAsync(LgdxRoleCreateDto role)
  {
    var roleJson = new StringContent(JsonSerializer.Serialize(role), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("Identity/Roles", roleJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdateRoleAsync(string id, LgdxRoleUpdateDto role)
  {
    var roleJson = new StringContent(JsonSerializer.Serialize(role), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"Identity/Roles/{id}", roleJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteRoleAsync(string id)
  {
    var response = await _httpClient.DeleteAsync($"Identity/Roles/{id}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchRolesAsync(string name)
  {
    var url = $"Identity/Roles?name={name}";
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