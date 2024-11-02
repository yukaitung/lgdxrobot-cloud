using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;

namespace LGDXRobot2Cloud.UI.Authorisation;

public class ValidateLgdxUserAccessAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
  private const string POLICY_PREFIX = "ValidateLgdxUserAccess";

  public string Area { get; set; }
  public string? Controller { get; set; }
  public ApiAccessLevel? Access { get; set; }

  public ValidateLgdxUserAccessAttribute(string area, string? controller = null, ApiAccessLevel? access = null)
  {
    Area = area;
    Controller = controller;
    Access = access;
    Policy = $"{POLICY_PREFIX}/{Area}";
    if (Controller != null)
    {
      Policy += $"/{Controller}";

    }
    if (Access != null)
    {
      Policy += $"/{Access}";
    }
  }

  public IEnumerable<IAuthorizationRequirement> GetRequirements()
  {
    return [new ValidateLgdxUserAccessRequirement(Area, Controller, Access)];
  }
}