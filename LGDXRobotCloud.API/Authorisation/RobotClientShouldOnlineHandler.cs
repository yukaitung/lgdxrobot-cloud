using System.Security.Claims;
using LGDXRobotCloud.API.Services.Navigation;
using Microsoft.AspNetCore.Authorization;

namespace LGDXRobotCloud.API.Authorisation;

public class RobotClientShouldOnlineHandler (
  IOnlineRobotsService OnlineRobotsService
) : AuthorizationHandler<RobotClientShouldOnlineRequirement>
{
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService ?? throw new ArgumentException(nameof(OnlineRobotsService));

  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RobotClientShouldOnlineRequirement requirement)
  {
    var robotClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
    if (robotClaim == null)
    {
      context.Fail();
      return;
    }
    if (Guid.TryParse(robotClaim.Value, out var robotId) && await _onlineRobotsService.IsRobotOnlineAsync(robotId))
    {
      context.Succeed(requirement);
      return;
    }
    else
    {
      context.Fail();
      return;
    }
  }
}