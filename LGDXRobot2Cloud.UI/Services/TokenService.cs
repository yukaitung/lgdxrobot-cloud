using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LGDXRobot2Cloud.UI.Services;

public interface ITokenService
{
  void Login(ClaimsPrincipal user, string accessToken, string refreshToken, DateTime accessTokenExpiresAt, DateTime refreshTokenExpiresAt);
  bool IsLoggedIn(ClaimsPrincipal user);
  string GetAccessToken(ClaimsPrincipal user);
  DateTime GetAccessTokenExpiresAt(ClaimsPrincipal user);
  string GetRefreshToken(ClaimsPrincipal user);
  DateTime GetRefreshTokenExpiresAt(ClaimsPrincipal user);
  void RefreshAccessToken(ClaimsPrincipal user, string accessToken, string refreshToken);
  SessionSettings GetSessionSettings(ClaimsPrincipal user);
  void UpdateSessionSettings(ClaimsPrincipal user, SessionSettings sessionSettings);
  void Logout(ClaimsPrincipal user);
}

public record SessionSettings
{
  public int? CurrentRealmId { get; set; }
}

public class TokenService : ITokenService
{
  private readonly ConcurrentDictionary<string, Token> Tokens = new();

  private record Token
  {
    public required string AccessToken { set; get; }
    public required string RefreshToken { set; get; }
    public required DateTime AccessTokenExpiresAt { set; get; }
    public required DateTime RefreshTokenExpiresAt { set; get; }
    public SessionSettings SessionSettings { set; get; } = new();
  }

  private static string GenerateAccessKey(ClaimsPrincipal user)
  {
    var userId = user.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
    return $"TokenService_{userId}";
  }

  public void Login(ClaimsPrincipal user, string accessToken, string refreshToken, DateTime accessTokenExpiresAt, DateTime refreshTokenExpiresAt)
  {
    Tokens.TryAdd(GenerateAccessKey(user), new Token { 
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      AccessTokenExpiresAt = accessTokenExpiresAt,
      RefreshTokenExpiresAt = refreshTokenExpiresAt
    });
  }

  public bool IsLoggedIn(ClaimsPrincipal user)
  {
    if (Tokens.TryGetValue(GenerateAccessKey(user), out Token? token))
    {
      return token != null && DateTime.UtcNow < token.RefreshTokenExpiresAt;
    }
    else
    {
      return false;
    }
  }

  public string GetAccessToken(ClaimsPrincipal user)
  {
    if (Tokens.TryGetValue(GenerateAccessKey(user), out Token? token))
    {
      return token!.AccessToken ?? string.Empty;
    }
    else
    {
      return string.Empty;
    }
  }

  public DateTime GetAccessTokenExpiresAt(ClaimsPrincipal user)
  {
    if (Tokens.TryGetValue(GenerateAccessKey(user), out Token? token))
    {
      return token!.AccessTokenExpiresAt;
    }
    else
    {
      return DateTime.MinValue;
    }
  }

  public string GetRefreshToken(ClaimsPrincipal user)
  {
    if (Tokens.TryGetValue(GenerateAccessKey(user), out Token? token))
    {
      return token!.RefreshToken ?? string.Empty;
    }
    else
    {
      return string.Empty;
    }
  }

  public DateTime GetRefreshTokenExpiresAt(ClaimsPrincipal user)
  {
    if (Tokens.TryGetValue(GenerateAccessKey(user), out Token? token))
    {
      return token!.RefreshTokenExpiresAt;
    }
    else
    {
      return DateTime.MinValue;
    }
  }

  public void RefreshAccessToken(ClaimsPrincipal user, string accessToken, string refreshToken)
  {
    if (Tokens.TryGetValue(GenerateAccessKey(user), out Token? token))
    {
      if (token != null)
      {
        Tokens.TryUpdate(GenerateAccessKey(user), new Token {
          AccessToken = accessToken,
          RefreshToken = refreshToken,
          AccessTokenExpiresAt = token.AccessTokenExpiresAt, 
          RefreshTokenExpiresAt = token.RefreshTokenExpiresAt
        }, token);
      }
    }
  }

  public SessionSettings GetSessionSettings(ClaimsPrincipal user)
  {
    if (Tokens.TryGetValue(GenerateAccessKey(user), out Token? token))
    {
      return token!.SessionSettings;
    }
    else
    {
      return new SessionSettings();
    }
  }

  public void UpdateSessionSettings(ClaimsPrincipal user, SessionSettings sessionSettings)
  {
    Tokens.TryGetValue(GenerateAccessKey(user), out Token? token);
    token!.SessionSettings = sessionSettings;
    Tokens.TryUpdate(GenerateAccessKey(user), token, token);
  }

  public void Logout(ClaimsPrincipal user)
  {
    Tokens.TryRemove(GenerateAccessKey(user), out _);
  }
}