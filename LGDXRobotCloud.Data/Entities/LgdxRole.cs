using Microsoft.AspNetCore.Identity;

namespace LGDXRobotCloud.Data.Entities;

public class LgdxRole : IdentityRole
{
  public string? Description { get; set; }
}