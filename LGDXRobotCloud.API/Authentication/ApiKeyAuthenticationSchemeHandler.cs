using System.Security.Claims;
using System.Text.Encodings.Web;
using LGDXRobotCloud.API.Services.Administration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LGDXRobotCloud.API.Authentication;

public class ApiKeyAuthenticationSchemeHandler(
    IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyService apiKeyService
  ) : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, encoder)
{
  private readonly IApiKeyService _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync() 
  {
    var apiKey = Context.Request.Headers["X-API-KEY"];
    if (string.IsNullOrWhiteSpace(apiKey) || await _apiKeyService.ValidateApiKeyAsync(apiKey!) == false) 
    {
      return AuthenticateResult.Fail("X-API-KEY is invalid");
    }

    var identity = new ClaimsIdentity([], Scheme.Name);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, Scheme.Name);
    return AuthenticateResult.Success(ticket);
  } 
}