using Microsoft.AspNetCore.Identity;

namespace LGDXRobot2Cloud.Data.Entities;

public class LgdxUser : IdentityUser
{
  [PersonalData]
  public string? Name { get; set; }
}