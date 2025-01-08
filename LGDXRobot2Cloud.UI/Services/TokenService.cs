using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LGDXRobot2Cloud.UI.Services;

public interface ITokenService
{
  void Login(ClaimsPrincipal user, string accessToken, string refreshToken);
  string GetAccessToken(ClaimsPrincipal user);
  string GetRefreshToken(ClaimsPrincipal user);
  void RefreshAccessToken(ClaimsPrincipal user, string accessToken, string refreshToken);
  void Logout(ClaimsPrincipal user);
}

public class TokenService : ITokenService
{
  private readonly ConcurrentDictionary<string, string> AccessTokens = new();
  private readonly ConcurrentDictionary<string, string> RefreshTokens = new();

  private static string GenerateAccessKey(ClaimsPrincipal user)
  {
    var userId = user.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
    var jti =  user.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
    return $"{userId}_{jti}";
  }

  public void Login(ClaimsPrincipal user, string accessToken, string refreshToken)
  {
    AccessTokens.TryAdd(GenerateAccessKey(user), accessToken);
    RefreshTokens.TryAdd(GenerateAccessKey(user), refreshToken);
  }

  public string GetAccessToken(ClaimsPrincipal user)
  {
    return AccessTokens.TryGetValue(GenerateAccessKey(user), out var accessToken) ? accessToken : string.Empty;
  }

  public string GetRefreshToken(ClaimsPrincipal user)
  {
    return RefreshTokens.TryGetValue(GenerateAccessKey(user), out var refreshToken) ? refreshToken : string.Empty;
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
  }
}