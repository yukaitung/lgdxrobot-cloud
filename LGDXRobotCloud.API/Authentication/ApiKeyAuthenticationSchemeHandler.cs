using System.Security.Claims;
using System.Text.Encodings.Web;
using LGDXRobotCloud.API.Services.Administration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LGDXRobotCloud.API.Authentication;

public class ApiKeyAuthenticationSchemeHandler(
    IApiKeyService apiKeyService,
    ILoggerFactory logger,
    IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
    IWebHostEnvironment webHostEnvironment,
    UrlEncoder encoder
  ) : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, encoder)
{
  private readonly IApiKeyService _apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
  private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync() 
  {
    string? apiKey = Context.Request.Headers["X-API-KEY"];
    var apiKeyId = await _apiKeyService.ValidateApiKeyAsync(apiKey);
    if (!_webHostEnvironment.IsDevelopment() && apiKeyId == null)
    {
      return AuthenticateResult.Fail("X-API-KEY is invalid");
    }
    
    Context.Items["ApiKeyId"] = apiKeyId;
    var identity = new ClaimsIdentity([], Scheme.Name);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, Scheme.Name);
    return AuthenticateResult.Success(ticket);
  } 
}