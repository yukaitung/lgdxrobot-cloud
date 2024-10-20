namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class LoginResponseDto
{
  public string AccessToken { get; set; } = string.Empty;
  public int ExpiresMins { get; set; }
}