using LGDXRobot2Cloud.UI.Authorisation;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace LGDXRobot2Cloud.UI;

public class LgdxApiClientFactory(AuthenticationStateProvider authenticationStateProvider, ITokenService tokenService, HttpClient httpClient)
{
  private readonly IAuthenticationProvider _authenticationProvider = new BaseBearerTokenAuthenticationProvider(new LgdxAccessTokenProvider(authenticationStateProvider, tokenService));
  private readonly HttpClient _httpClient = httpClient;

  public LgdxApiClient GetClient() 
  {
    return new LgdxApiClient(new HttpClientRequestAdapter(_authenticationProvider, httpClient: _httpClient));
  }
}