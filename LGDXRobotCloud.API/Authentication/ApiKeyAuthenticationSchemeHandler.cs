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
    if ((!_webHostEnvironment.IsDevelopment() && apiKeyId == null) || // API Key is invalid in production
        (_webHostEnvironment.IsDevelopment() && !string.IsNullOrWhiteSpace(apiKey) && apiKeyId == null)) // API Key (if provided) is incorrect in development
    {
      return AuthenticateResult.Fail("The API Key is invalid");
    }
    
    Context.Items["ApiKeyId"] = apiKeyId;
    var identity = new ClaimsIdentity([], Scheme.Name);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, Scheme.Name);
    return AuthenticateResult.Success(ticket);
  } 
}