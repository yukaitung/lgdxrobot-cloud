using LGDXRobotCloud.UI.Authorisation;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace LGDXRobotCloud.UI;

public class LgdxApiClientFactory(AuthenticationStateProvider authenticationStateProvider, ITokenService tokenService, HttpClient httpClient)
{
  private readonly IAuthenticationProvider _authenticationProvider = new BaseBearerTokenAuthenticationProvider(new LgdxAccessTokenProvider(authenticationStateProvider, tokenService));
  private readonly HttpClient _httpClient = httpClient;

  public LgdxApiClient GetClient() 
  {
    return new LgdxApiClient(new HttpClientRequestAdapter(_authenticationProvider, httpClient: _httpClient));
  }
}