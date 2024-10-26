using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.Identity;

namespace LGDXRobot2Cloud.UI.Services;

public interface IUserService
{
  Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public class UserService : IUserService
{
  private readonly HttpClient _httpClient;
  private readonly JsonSerializerOptions _jsonSerializerOptions;

  public UserService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<LoginResponse?> LoginAsync(LoginRequest request)
  {
    var json = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("/Identity/User/login", json);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<LoginResponse>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
    {
      return null;
    }
  }
}