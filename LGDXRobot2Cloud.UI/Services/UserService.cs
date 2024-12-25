using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Data.Models.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public interface IUserService
{
  Task<LgdxUser?> GetUserAsync();
  Task<bool> UpdateUserAsync(LgdxUserUpdateDto user);
  Task<bool> UpdatePasswordAsync(UpdatePasswordRequest updatePasswordRequest);
}

public sealed class UserService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient
  ) : BaseService(authenticationStateProvider, httpClient), IUserService
{
  public async Task<LgdxUser?> GetUserAsync()
  {
    var response = await _httpClient.GetAsync("Identity/User");
    var user = JsonSerializer.Deserialize<LgdxUser>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return user;
  }

  public async Task<bool> UpdateUserAsync(LgdxUserUpdateDto user)
  {
    var userJson = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
    var response = await _httpClient.PutAsync("Identity/User", userJson);
    return response.IsSuccessStatusCode;
  }

  public async Task<bool> UpdatePasswordAsync(UpdatePasswordRequest updatePasswordRequest)
  {
    var userJson = new StringContent(JsonSerializer.Serialize(updatePasswordRequest), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("Identity/User/Password", userJson);
    return response.IsSuccessStatusCode;
  }
}