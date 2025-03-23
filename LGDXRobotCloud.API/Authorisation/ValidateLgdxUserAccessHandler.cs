using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LGDXRobotCloud.API.Authorisation;

public class ValidateLgdxUserAccessHandler(
  IHttpContextAccessor httpContextAccessor
) : AuthorizationHandler<ValidateLgdxUserAccessRequirement>
{
  private readonly HttpContext _httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentException(nameof(httpContextAccessor));

  private static bool IsValidApplication(string str)
  {
    return string.Equals(str, "LGDXRobotCloud.API", StringComparison.CurrentCultureIgnoreCase);
  }

  private bool HasAccess(string str)
  {
    string method = _httpContext.Request.Method;
    if (string.Equals(str, "FullAccess", StringComparison.CurrentCultureIgnoreCase))
    {
      // Has full access
      return true;
    }
    if (string.Equals(str, "Read", StringComparison.CurrentCultureIgnoreCase))
    {
      // Has read access
      return string.Equals(method, "GET", StringComparison.CurrentCultureIgnoreCase);
    }
    else if (string.Equals(str, "Write", StringComparison.CurrentCultureIgnoreCase))
    {
      // Has write access
      return string.Equals(method, "POST", StringComparison.CurrentCultureIgnoreCase) ||
             string.Equals(method, "PUT", StringComparison.CurrentCultureIgnoreCase);
    }
    else if (string.Equals(str, "Delete", StringComparison.CurrentCultureIgnoreCase))
    {
      // Has delete access
      return string.Equals(method, "DELETE", StringComparison.CurrentCultureIgnoreCase);
    }
    return false;
  }

  private bool HasAreaAccess(string str)
  {
    string? area = _httpContext.Request.RouteValues["area"]?.ToString();
    if (string.IsNullOrWhiteSpace(area))
    {
      return false;
    }
    return string.Equals(area, str, StringComparison.CurrentCultureIgnoreCase);
  }

  private bool HasControllerAccess(string str)
  {
    string? controller = _httpContext.Request.RouteValues["controller"]?.ToString();
    if (string.IsNullOrWhiteSpace(controller))
    {
      return false;
    }
    return string.Equals(controller, str, StringComparison.CurrentCultureIgnoreCase);
  }

  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidateLgdxUserAccessRequirement requirement)
  {
    List<Claim> scopes = context.User.FindAll(c => c.Type == "scope").ToList();
    // Extract scope
    // format: LGDXRobotCloud.API/Area/Controller/<Access>
    foreach (Claim scope in scopes)
    {
      var scopeSplit = scope.Value.Split("/");
      if (scopeSplit.Length == 2)
      {
        // format: LGDXRobotCloud.API/<Access>
        if (IsValidApplication(scopeSplit[0]) && HasAccess(scopeSplit[1]))
        {
          context.Succeed(requirement);
          return Task.CompletedTask;
        }
      }
      else if (scopeSplit.Length == 3)
      {
        // format: LGDXRobotCloud.API/Area/<Access>
        if (IsValidApplication(scopeSplit[0]) && HasAreaAccess(scopeSplit[1]) && HasAccess(scopeSplit[2]))
        {
          context.Succeed(requirement);
          return Task.CompletedTask;
        }
      }
      else if (scopeSplit.Length == 4)
      {
        // format: LGDXRobotCloud.API/Area/Controller/<Access>
        if (IsValidApplication(scopeSplit[0]) && 
            HasAreaAccess(scopeSplit[1]) && 
            HasControllerAccess(scopeSplit[2]) && 
            HasAccess(scopeSplit[3]))
        {
          context.Succeed(requirement);
          return Task.CompletedTask;
        }
      }
    }
    context.Fail();
    return Task.CompletedTask;
  }
}