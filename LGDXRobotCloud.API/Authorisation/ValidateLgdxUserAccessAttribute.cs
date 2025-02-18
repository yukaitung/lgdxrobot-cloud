using Microsoft.AspNetCore.Authorization;

namespace LGDXRobotCloud.API.Authorisation;

public class ValidateLgdxUserAccessAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
  public IEnumerable<IAuthorizationRequirement> GetRequirements()
  {
    return [new ValidateLgdxUserAccessRequirement()];
  }
}