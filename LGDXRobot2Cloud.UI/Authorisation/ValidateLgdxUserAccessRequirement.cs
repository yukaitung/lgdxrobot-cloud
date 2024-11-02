using LGDXRobot2Cloud.Utilities.Enums;
using Microsoft.AspNetCore.Authorization;

namespace LGDXRobot2Cloud.UI.Authorisation;

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