using Microsoft.AspNetCore.Identity;

namespace LGDXRobot2Cloud.UI.Models;

public class LgdxUserInfo
{
  public string Id { get; set; } = null!;

  public string Username { get; set; } = null!;
  
  [PersonalData]
  public string? Name { get; set; }
  
  [PersonalData]
  public string Email { get; set; } = null!;

  [PersonalData]
  public string AccessToken { get; set; } = null!;
}