using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LGDXRobot2Cloud.UI.Authorisation;

public class ValidateLgdxUserAccesPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
  private const string POLICY_PREFIX = "ValidateLgdxUserAccess";
  private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; } = new DefaultAuthorizationPolicyProvider(options);

  public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => 
    Task.FromResult(new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());

  public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => 
    Task.FromResult<AuthorizationPolicy?>(null);

  public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
  {
    var scopeSplit = policyName.Split("/");
    
    if (scopeSplit.Length > 1 && string.Equals(scopeSplit[0], POLICY_PREFIX, StringComparison.CurrentCultureIgnoreCase))
    {
      var policy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme);
      if (scopeSplit.Length == 2)
      {
        policy.AddRequirements(new ValidateLgdxUserAccessRequirement(scopeSplit[1]));
      }
      else if (scopeSplit.Length == 3)
      {
        policy.AddRequirements(new ValidateLgdxUserAccessRequirement(scopeSplit[1], scopeSplit[2]));
      }
      else if (scopeSplit.Length == 4)
      {
        policy.AddRequirements(new ValidateLgdxUserAccessRequirement(scopeSplit[1], scopeSplit[2], (ApiAccessLevel)Enum.Parse(typeof(ApiAccessLevel), scopeSplit[3])));
      }
      return Task.FromResult((AuthorizationPolicy?) policy.Build());
    }

    return BackupPolicyProvider.GetPolicyAsync(policyName);
  }
}