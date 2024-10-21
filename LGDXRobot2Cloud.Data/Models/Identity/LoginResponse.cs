namespace LGDXRobot2Cloud.Data.Models.Identity;

public class LoginResponse
{
  public string AccessToken { get; set; } = string.Empty;
  public int ExpiresMins { get; set; }
}