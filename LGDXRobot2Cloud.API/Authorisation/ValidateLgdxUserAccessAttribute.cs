using Microsoft.AspNetCore.Authorization;

namespace LGDXRobot2Cloud.API.Authorisation;

public class ValidateLgdxUserAccessAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
  public IEnumerable<IAuthorizationRequirement> GetRequirements()
  {
    return [new ValidateLgdxUserAccessRequirement()];
  }
}