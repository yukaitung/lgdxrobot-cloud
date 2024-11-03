using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LGDXRobot2Cloud.UI.Authorisation;

public class ValidateLgdxUserAccessHandler : AuthorizationHandler<ValidateLgdxUserAccessRequirement>
{
  private static bool IsValidApplication(string str)
  {
    return string.Equals(str, "LGDXRobot2Cloud.API", StringComparison.CurrentCultureIgnoreCase);
  }

  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidateLgdxUserAccessRequirement requirement)
  {
    List<Claim> scopes = context.User.Claims.Where(c => c.Type == "scope").ToList();
    // Extract scope
    // format: LGDXRobot2Cloud.API/Area/Controller/<Access>
    // If the Controller / Access is unspecified, it will not be checked
    foreach (Claim scope in scopes)
    {
      var scopeSplit = scope.Value.Split("/");
      if (scopeSplit.Length == 0 || !IsValidApplication(scopeSplit[0]))
      {
        // Not LgdxRobot2Cloud.API scope
        continue;
      }

      bool hasAccess = true;
      if (requirement.Area != null)
      {
        if (!(scopeSplit.Length <= 2 ||
            string.Equals(scopeSplit[1], requirement.Area, StringComparison.CurrentCultureIgnoreCase)))
        {
          hasAccess = false;
        }
      }
      if (requirement.Controller != null)
      {
        if (!(scopeSplit.Length <= 3 ||
            string.Equals(scopeSplit[2], requirement.Controller, StringComparison.CurrentCultureIgnoreCase)))
        {
          hasAccess = false;
        }
      }
      if (requirement.Access != null)
      {
        if (!string.Equals(scopeSplit[^1], ApiAccessLevel.FullAccess.ToString(), StringComparison.CurrentCultureIgnoreCase))
        {
          switch (requirement.Access)
          {
            case ApiAccessLevel.Read:
              if (!string.Equals(scopeSplit[^1], ApiAccessLevel.Read.ToString(), StringComparison.CurrentCultureIgnoreCase))
              {
                hasAccess = false;
              }
              break;
            case ApiAccessLevel.Write:
              if (!string.Equals(scopeSplit[^1], ApiAccessLevel.Write.ToString(), StringComparison.CurrentCultureIgnoreCase))
              {
                hasAccess = false;;
              }
              break;
            case ApiAccessLevel.Delete:
              if (!string.Equals(scopeSplit[^1], ApiAccessLevel.Delete.ToString(), StringComparison.CurrentCultureIgnoreCase))
              {
                hasAccess = false;
              }
              break;
            default:
              hasAccess = false;
              break;
          }
        }
      }
      if (hasAccess)
      {
        context.Succeed(requirement);
        return Task.CompletedTask;
      }
    }

    context.Fail();
    return Task.CompletedTask;
  }
}