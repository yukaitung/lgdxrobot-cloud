using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;

namespace LGDXRobotCloud.UI.Authorisation;

public class ValidateLgdxUserAccessRequirement(
  string area, 
  string? controller = null, 
  ApiAccessLevel? access = null
  ) : IAuthorizationRequirement
{
  public string Area { get; set; } = area;
  public string? Controller { get; set; } = controller;
  public ApiAccessLevel? Access { get; set; } = access;
}