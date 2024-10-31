using LGDXRobot2Cloud.Data.Models.Identity;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface IUsersService
{
  Task<(IEnumerable<LgdxUser>?, PaginationHelper?)> GetUsersAsync(string? name = null, int pageNum = 1, int pageSize = 10);
  Task<LgdxUser?> GetUserAsync(string userId);
  Task<bool> AddUserAsync(LgdxUserCreateDto user);
  Task<bool> UpdateUserAsync(string userId, LgdxUserUpdateDto user);
  Task<bool> DeleteUserAsync(string userId);
  Task<string> SearchRolessAsync(string name);
}

public sealed class UsersService(
  AuthenticationStateProvider authenticationStateProvider, 
  HttpClient httpClient) : BaseService(authenticationStateProvider, httpClient), IUsersService
{
  public async Task<(IEnumerable<LgdxUser>?, PaginationHelper?)> GetUsersAsync(string? name = null, int pageNum = 1, int pageSize = 10)
  {
    var url = name != null ? $"Identity/Users?name={name}&pageNumber={pageNum}&pageSize={pageSize}" : $"Identity/Users?pageNumber={pageNum}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var users = JsonSerializer.Deserialize<IEnumerable<LgdxUser>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (users, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<LgdxUser?> GetUserAsync(string userId)
  {
    var response = await _httpClient.GetAsync($"Identity/Users/{userId}");
    var user = JsonSerializer.Deserialize<LgdxUser>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return user;
  }

  public async Task<bool> AddUserAsync(LgdxUserCreateDto user)
  {
    var userJson = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("Identity/Users", userJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdateUserAsync(string userId, LgdxUserUpdateDto user)
  {
    var userJson = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync($"Identity/Users/{userId}", userJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> DeleteUserAsync(string userId)
  {
    var response = await _httpClient.DeleteAsync($"Identity/Users/{userId}");
    return response.IsSuccessStatusCode;
  }

  public async Task<string> SearchRolessAsync(string name)
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