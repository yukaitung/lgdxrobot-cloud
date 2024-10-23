using Microsoft.AspNetCore.Identity;

namespace LGDXRobot2Cloud.Data.Entities;

public class LgdxRole : IdentityRole
{
  public string? Description { get; set; }
}