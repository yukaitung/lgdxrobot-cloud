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
  void Logout(ClaimsPrincipal user);
}

public class TokenService : ITokenService
{
  private readonly ConcurrentDictionary<string, string> AccessTokens = new();
  private readonly ConcurrentDictionary<string, string> RefreshTokens = new();
  private readonly ConcurrentDictionary<string, DateTime> AccessTokenExpiresAt = new();
  private readonly ConcurrentDictionary<string, DateTime> RefreshTokenExpiresAt = new();

  private static string GenerateAccessKey(ClaimsPrincipal user)
  {
    var userId = user.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
    return $"{userId}";
  }

  public void Login(ClaimsPrincipal user, string accessToken, string refreshToken, DateTime accessTokenExpiresAt, DateTime refreshTokenExpiresAt)
  {
    AccessTokens.TryAdd(GenerateAccessKey(user), accessToken);
    RefreshTokens.TryAdd(GenerateAccessKey(user), refreshToken);
    AccessTokenExpiresAt.TryAdd(GenerateAccessKey(user), accessTokenExpiresAt);
    RefreshTokenExpiresAt.TryAdd(GenerateAccessKey(user), refreshTokenExpiresAt);
  }

  public bool IsLoggedIn(ClaimsPrincipal user)
  {
    return AccessTokens.ContainsKey(GenerateAccessKey(user));
  }

  public string GetAccessToken(ClaimsPrincipal user)
  {
    return AccessTokens.TryGetValue(GenerateAccessKey(user), out var accessToken) ? accessToken : string.Empty;
  }

  public DateTime GetAccessTokenExpiresAt(ClaimsPrincipal user)
  {
    return AccessTokenExpiresAt.TryGetValue(GenerateAccessKey(user), out var accessTokenExpiresAt) ? accessTokenExpiresAt : DateTime.MinValue;
  }

  public string GetRefreshToken(ClaimsPrincipal user)
  {
    return RefreshTokens.TryGetValue(GenerateAccessKey(user), out var refreshToken) ? refreshToken : string.Empty;
  }

  public DateTime GetRefreshTokenExpiresAt(ClaimsPrincipal user)
  {
    return RefreshTokenExpiresAt.TryGetValue(GenerateAccessKey(user), out var refreshTokenExpiresAt) ? refreshTokenExpiresAt : DateTime.MinValue;
  }

  public void RefreshAccessToken(ClaimsPrincipal user, string accessToken, string refreshToken)
  {
    AccessTokens.TryUpdate(GenerateAccessKey(user), accessToken, AccessTokens[GenerateAccessKey(user)]);
    RefreshTokens.TryUpdate(GenerateAccessKey(user), refreshToken, RefreshTokens[GenerateAccessKey(user)]);
  }

  public void Logout(ClaimsPrincipal user)
  {
    AccessTokens.TryRemove(GenerateAccessKey(user), out _);
    RefreshTokens.TryRemove(GenerateAccessKey(user), out _);
    AccessTokenExpiresAt.TryRemove(GenerateAccessKey(user), out _);
    RefreshTokenExpiresAt.TryRemove(GenerateAccessKey(user), out _);
  }
}