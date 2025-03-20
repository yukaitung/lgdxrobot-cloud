using Microsoft.AspNetCore.Authorization;

namespace LGDXRobotCloud.API.Authorisation;

public class RobotClientShouldOnlineAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
  public IEnumerable<IAuthorizationRequirement> GetRequirements()
  {
    return [new RobotClientShouldOnlineRequirement()];
  }
}